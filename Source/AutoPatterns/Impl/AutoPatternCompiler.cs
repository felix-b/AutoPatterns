using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Impl
{
    public sealed class AutoPatternCompiler
    {
        public const string FactoryMethodNamePrefix = "FactoryMethod__";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly string _namespaceName;
        private readonly Func<MetaCompilerContext, IAutoPatternTemplate[]> _onBuildPipeline;
        private readonly ConcurrentDictionary<TypeKey, MemberDeclarationSyntax> _syntaxCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutoPatternCompiler(AutoPatternLibrary library, string namespaceName, Func<MetaCompilerContext, IAutoPatternTemplate[]> onBuildPipeline)
        {
            _namespaceName = namespaceName;
            _onBuildPipeline = onBuildPipeline;
            _syntaxCache = new ConcurrentDictionary<TypeKey, MemberDeclarationSyntax>();

            library.AddCompiler(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildSyntax(TypeKey typeKey, Type baseType = null, Type primaryInterface = null)
        {
            BuildSyntax(
                typeKey, 
                baseType, 
                primaryInterfaces: primaryInterface != null ? new[] { primaryInterface } : null, 
                secondaryInterfaces: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void BuildSyntax(TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            _syntaxCache.GetOrAdd(
                typeKey, 
                k => BuildNewClassSyntax(typeKey, baseType, primaryInterfaces, secondaryInterfaces));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void TakeAllSyntax(out MemberDeclarationSyntax[] members)
        {
            members = _syntaxCache.Values.ToArray();
            _syntaxCache.Clear();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NamespaceName => _namespaceName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax BuildNewClassSyntax(
            TypeKey typeKey, 
            Type baseType, 
            Type[] primaryInterfaces, 
            Type[] secondaryInterfaces)
        {
            var context = new MetaCompilerContext(this, typeKey, baseType, primaryInterfaces, secondaryInterfaces);
            var pipeline = _onBuildPipeline(context);

            PreBuildSyntax(context);

            for (int i = 0; i < pipeline.Length; i++)
            {
                pipeline[i].Apply(context);
            }

            var syntax = GetFinalSyntax(context);
            return syntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PreBuildSyntax(MetaCompilerContext context)
        {
            EnsureMetadataReference(typeof(object));

            if (context.Input.BaseType != null)
            {
                AddBaseType(context.Input.BaseType, context);
            }

            foreach (var interfaceType in context.Input.PrimaryInterfaces)
            {
                AddBaseType(interfaceType, context);
            }

            foreach (var interfaceType in context.Input.SecondaryInterfaces)
            {
                AddBaseType(interfaceType, context);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddBaseType(Type type, MetaCompilerContext context)
        {
            EnsureMetadataReference(type);
            context.Output.BaseTypes.Add(SimpleBaseType(SyntaxHelper.GetTypeSyntax(type)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax GetFinalSyntax(MetaCompilerContext context)
        {
            AddFactoryMethods(context);

            return 
                NamespaceDeclaration(IdentifierName(context.Output.ClassNamespace))
                .WithUsings(
                    SingletonList<UsingDirectiveSyntax>(UsingDirective(IdentifierName("System")))
                )
                .WithMembers(
                    List<MemberDeclarationSyntax>(
                        new MemberDeclarationSyntax[] {
                            ClassDeclaration(context.Output.ClassName)
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(context.Output.BaseTypes)))
                                .WithMembers(List<MemberDeclarationSyntax>(
                                    context.Output.GetAllMembers()
                                ))
                        }
                    )
                );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddFactoryMethods(MetaCompilerContext context)
        {
            var constructorList = context.Output.Constructors;

            if (constructorList.Count > 0)
            {
                for (int index = 0 ; index < constructorList.Count; index++)
                {
                    AddFactoryMethod(context, constructorList[index], index);
                }
            }
            else
            {
                AddDefaultFactoryMethod(context);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddFactoryMethod(MetaCompilerContext context, ConstructorDeclarationSyntax constructor, int index)
        {
            var factoryMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)), Identifier($"FactoryMethod__{index}"))
                .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                .WithParameterList(constructor.ParameterList)
                .WithBody(Block(SingletonList<StatementSyntax>(
                    ReturnStatement(
                        ObjectCreationExpression(IdentifierName(context.Output.ClassName))
                            .WithArgumentList(SyntaxHelper.CopyParametersToArguments(constructor.ParameterList)))
                 )));

            context.Output.Methods.Add(factoryMethod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddDefaultFactoryMethod(MetaCompilerContext context)
        {
            var factoryMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)), Identifier("FactoryMethod__0"))
                .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                .WithBody(Block(SingletonList<StatementSyntax>(
                    ReturnStatement(ObjectCreationExpression(IdentifierName(context.Output.ClassName)).WithArgumentList(ArgumentList())))
                ));

            context.Output.Methods.Add(factoryMethod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly object _s_referenceCacheSyncRoot = new object();
        private static ImmutableDictionary<string, MetadataReference> _s_referenceCache = ImmutableDictionary.Create<string, MetadataReference>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static bool CompileAssembly(AutoPatternCompiler[] compilers, string assemblyName, out byte[] assemblyBytes)
        {
            if (compilers == null)
            {
                throw new ArgumentNullException(nameof(compilers));
            }

            if (compilers.Length == 0)
            {
                throw new ArgumentException("At least one compiler is required.", nameof(compilers));
            }

            var allMembers = new List<MemberDeclarationSyntax>();

            foreach (var compiler in compilers)
            {
                IncludeExportsFromCompiler(compiler, allMembers);
            }

            if (allMembers.Count == 0)
            {
                assemblyBytes = null;
                return false;
            }

            var syntaxTree = SyntaxTree(CompilationUnit()
                .WithMembers(List(allMembers))
                .NormalizeWhitespace());

            //Console.WriteLine(syntaxTree.ToString());

            var compilation = CSharpCompilation
                .Create(assemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(_s_referenceCache.Values)
                .AddSyntaxTrees(syntaxTree);

            assemblyBytes = EmitAssemblyBytes(compilation);
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static bool CompileAndLoadAssembly(AutoPatternCompiler[] compilers, string assemblyName, out Assembly compiledAssembly)
        {
            byte[] assemblyBytes;

            if (CompileAssembly(compilers, assemblyName, out assemblyBytes))
            {
                compiledAssembly = Assembly.Load(assemblyBytes);
                return true;
            }

            compiledAssembly = null;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static MetadataReference EnsureMetadataReference(Assembly assembly)
        {
            var cacheKey = assembly.FullName;
            MetadataReference reference;

            if (!_s_referenceCache.TryGetValue(cacheKey, out reference))
            {
                lock (_s_referenceCacheSyncRoot)
                {
                    if (!_s_referenceCache.TryGetValue(cacheKey, out reference))
                    {
                        reference = MetadataReference.CreateFromFile(assembly.Location);
                        _s_referenceCache = _s_referenceCache.Add(cacheKey, reference);
                    }
                }
            }

            return reference;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static MetadataReference EnsureMetadataReference(Type type)
        {
            return EnsureMetadataReference(type.GetTypeInfo().Assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void IncludeExportsFromCompiler(
            AutoPatternCompiler compiler, 
            List<MemberDeclarationSyntax> allMembers)
        {
            MemberDeclarationSyntax[] members;
            compiler.TakeAllSyntax(out members);
            allMembers.AddRange(members);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static byte[] EmitAssemblyBytes(CSharpCompilation compilation)
        {
            using (var output = new MemoryStream())
            {
                EmitResult result = compilation.Emit(output);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = 
                        result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    throw new Exception(
                        "Compile failed:" + 
                        System.Environment.NewLine + 
                        string.Join(System.Environment.NewLine, failures.Select(f => $"{f.Id}: {f.GetMessage()}")));
                }

                return output.ToArray();
            }
        }
    }
}
