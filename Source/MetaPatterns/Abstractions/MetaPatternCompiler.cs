using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MetaPatterns.Extensions;
using MetaPatterns.Impl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MetaPatterns.Abstractions
{
    public abstract class MetaPatternCompiler
    {
        public const string FactoryMethodNamePrefix = "FactoryMethod__";
        public const string DefaultAssemblyName = "MetaPatterns.GeneratedTypes";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly IMetaPatternCompilerPlatform _platform;
        private readonly ConcurrentDictionary<TypeKey, MemberDeclarationSyntax> _syntaxCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MetaPatternCompiler(IMetaPatternCompilerPlatform platform)
        {
            _platform = platform;
            _syntaxCache = new ConcurrentDictionary<TypeKey, MemberDeclarationSyntax>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BuildSyntax(TypeKey typeKey, Type baseType = null, Type primaryInterface = null)
        {
            BuildSyntax(
                typeKey, 
                baseType, 
                primaryInterfaces: primaryInterface != null ? new[] { primaryInterface } : null, 
                secondaryInterfaces: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected void BuildSyntax(TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            _syntaxCache.GetOrAdd(
                typeKey, 
                k => BuildNewClassSyntax(typeKey, baseType, primaryInterfaces, secondaryInterfaces));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void ExportAllSyntaxes(out MemberDeclarationSyntax[] members)
        {
            members = _syntaxCache.Values.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract IMetaPatternTemplate[] BuildPipeline(MetaPatternCompilerContext context);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected internal virtual string NamespaceName => this.GetType().Name.TrimSuffix("Compiler");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected internal IMetaPatternCompilerPlatform Platform => _platform;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax BuildNewClassSyntax(
            TypeKey typeKey, 
            Type baseType, 
            Type[] primaryInterfaces, 
            Type[] secondaryInterfaces)
        {
            var context = new MetaPatternCompilerContext(this, typeKey, baseType, primaryInterfaces, secondaryInterfaces);
            var pipeline = BuildPipeline(context);

            PreBuildSyntax(context);

            for (int i = 0; i < pipeline.Length; i++)
            {
                pipeline[i].Apply(context);
            }

            var syntax = GetFinalSyntax(context);
            return syntax;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PreBuildSyntax(MetaPatternCompilerContext context)
        {
            EnsureMetadataReference(_platform, typeof(object));

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

        private void AddBaseType(Type type, MetaPatternCompilerContext context)
        {
            EnsureMetadataReference(_platform, type);
            context.Output.BaseTypes.Add(SimpleBaseType(SyntaxHelper.GetTypeSyntax(type)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax GetFinalSyntax(MetaPatternCompilerContext context)
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

        private void AddFactoryMethods(MetaPatternCompilerContext context)
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

        private void AddFactoryMethod(MetaPatternCompilerContext context, ConstructorDeclarationSyntax constructor, int index)
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

        private void AddDefaultFactoryMethod(MetaPatternCompilerContext context)
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

        public static byte[] CompileAssembly(MetaPatternCompiler[] compilers, string assemblyName = null)
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

            var syntaxTree = SyntaxTree(CompilationUnit().WithMembers(List<MemberDeclarationSyntax>(allMembers)).NormalizeWhitespace());
            var platform = compilers[0].Platform;

            platform.Print(syntaxTree.ToString());

            var compilation = CSharpCompilation
                .Create(assemblyName ?? DefaultAssemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(_s_referenceCache.Values)
                .AddSyntaxTrees(syntaxTree);

            var assemblyBytes = EmitAssemblyBytes(compilation);
            return assemblyBytes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Assembly CompileAndLoadAssembly(MetaPatternCompiler[] compilers)
        {
            var bytes = CompileAssembly(compilers);
            var platform = compilers[0].Platform;
            return platform.LoadAssemblyFromBytes(bytes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected static MetadataReference EnsureMetadataReference(IMetaPatternCompilerPlatform platform, Assembly assembly)
        {
            var cacheKey = assembly.FullName;
            MetadataReference reference;

            if (!_s_referenceCache.TryGetValue(cacheKey, out reference))
            {
                lock (_s_referenceCacheSyncRoot)
                {
                    if (!_s_referenceCache.TryGetValue(cacheKey, out reference))
                    {
                        reference = platform.CreateMetadataReference(assembly);
                        _s_referenceCache = _s_referenceCache.Add(cacheKey, reference);
                    }
                }
            }

            return reference;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected static MetadataReference EnsureMetadataReference(IMetaPatternCompilerPlatform platform, Type type)
        {
            return EnsureMetadataReference(platform, type.GetTypeInfo().Assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void IncludeExportsFromCompiler(
            MetaPatternCompiler compiler, 
            List<MemberDeclarationSyntax> allMembers)
        {
            MemberDeclarationSyntax[] members;
            compiler.ExportAllSyntaxes(out members);
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
