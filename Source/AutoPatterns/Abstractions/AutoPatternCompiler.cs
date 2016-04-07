using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoPatterns.Extensions;
using AutoPatterns.Impl;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Abstractions
{
    public abstract class AutoPatternCompiler
    {
        public const string FactoryMethodNamePrefix = "FactoryMethod__";
        public const string DefaultAssemblyName = "MetaPatterns.GeneratedTypes";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly ConcurrentDictionary<TypeKey, MemberDeclarationSyntax> _syntaxCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AutoPatternCompiler()
        {
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

        protected abstract IAutoPatternTemplate[] BuildPipeline(MetaCompilerContext context);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected internal virtual string NamespaceName => this.GetType().Name.TrimSuffix("Compiler");

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax BuildNewClassSyntax(
            TypeKey typeKey, 
            Type baseType, 
            Type[] primaryInterfaces, 
            Type[] secondaryInterfaces)
        {
            var context = new MetaCompilerContext(this, typeKey, baseType, primaryInterfaces, secondaryInterfaces);
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
            context.Output.BaseTypes.Add(SyntaxFactory.SimpleBaseType(SyntaxHelper.GetTypeSyntax(type)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax GetFinalSyntax(MetaCompilerContext context)
        {
            AddFactoryMethods(context);

            return 
                SyntaxFactory.NamespaceDeclaration(IdentifierName(context.Output.ClassNamespace))
                .WithUsings(
                    SyntaxFactory.SingletonList<UsingDirectiveSyntax>(SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System")))
                )
                .WithMembers(
                    SyntaxFactory.List<MemberDeclarationSyntax>(
                        new MemberDeclarationSyntax[] {
                            ClassDeclaration(context.Output.ClassName)
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .WithBaseList(SyntaxFactory.BaseList(SeparatedList<BaseTypeSyntax>(context.Output.BaseTypes)))
                                .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(
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
            var factoryMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), SyntaxFactory.Identifier($"FactoryMethod__{index}"))
                .WithModifiers(SyntaxFactory.TokenList(new[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword) }))
                .WithParameterList(constructor.ParameterList)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.ObjectCreationExpression(IdentifierName(context.Output.ClassName))
                            .WithArgumentList(SyntaxHelper.CopyParametersToArguments(constructor.ParameterList)))
                 )));

            context.Output.Methods.Add(factoryMethod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddDefaultFactoryMethod(MetaCompilerContext context)
        {
            var factoryMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)), SyntaxFactory.Identifier("FactoryMethod__0"))
                .WithModifiers(SyntaxFactory.TokenList(new[] { SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword) }))
                .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                    SyntaxFactory.ReturnStatement(SyntaxFactory.ObjectCreationExpression(IdentifierName(context.Output.ClassName)).WithArgumentList(SyntaxFactory.ArgumentList())))
                ));

            context.Output.Methods.Add(factoryMethod);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly object _s_referenceCacheSyncRoot = new object();
        private static ImmutableDictionary<string, MetadataReference> _s_referenceCache = ImmutableDictionary.Create<string, MetadataReference>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static byte[] CompileAssembly(AutoPatternCompiler[] compilers, string assemblyName = null)
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

            var syntaxTree = SyntaxFactory.SyntaxTree(SyntaxFactory.CompilationUnit().WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>(allMembers)).NormalizeWhitespace());

            //Console.WriteLine(syntaxTree.ToString());

            var compilation = CSharpCompilation
                .Create(assemblyName ?? DefaultAssemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(_s_referenceCache.Values)
                .AddSyntaxTrees(syntaxTree);

            var assemblyBytes = EmitAssemblyBytes(compilation);
            return assemblyBytes;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Assembly CompileAndLoadAssembly(AutoPatternCompiler[] compilers)
        {
            var bytes = CompileAssembly(compilers);
            var assembly = Assembly.Load(bytes);
            return assembly;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected static MetadataReference EnsureMetadataReference(Assembly assembly)
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

        internal protected static MetadataReference EnsureMetadataReference(Type type)
        {
            return EnsureMetadataReference(type.GetTypeInfo().Assembly);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void IncludeExportsFromCompiler(
            AutoPatternCompiler compiler, 
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
