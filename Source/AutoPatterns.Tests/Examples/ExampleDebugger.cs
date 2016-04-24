using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using AutoPatterns.DesignTime;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    [DataContract(Name = MetaProgram.Constant.String1, Namespace = MetaProgram.Constant.String2)]
    public partial class ExampleDebugger : ExampleAncestors.ITryDebugging
    {
        [MetaProgram.Annotation.NewMember]
        public void TryDebugging()
        {
            TestLibrary.Platform.ConsoleWriteLine("HELLO WORLD!");
            System.Diagnostics.Debug.WriteLine("HELLO DEBUG!");
            System.Diagnostics.Debugger.Launch();
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public partial class ExampleDebugger : IPatternTemplate
    {
        void IPatternTemplate.Apply(PatternWriterContext context)
        {
            context.Output.ClassWriter.AddBaseType(typeof(ExampleAncestors.ITryDebugging));

            TryDebugging__Apply(
                context, 
                typeof(ExampleAncestors.ITryDebugging).GetTypeInfo().GetDeclaredMethod(nameof(ExampleAncestors.ITryDebugging.TryDebugging)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void TryDebugging__Apply(PatternWriterContext context, MethodInfo declaration)
        {
            context.Library.EnsureMetadataReference(typeof(System.Diagnostics.Debug));
            context.Library.EnsureMetadataReference(typeof(System.Diagnostics.Debugger));

            var tryDebuggingMethod = context.Output.ClassWriter.AddPublicVoidMethod(nameof(ExampleAncestors.ITryDebugging.TryDebugging), declaration);

            tryDebuggingMethod.Syntax =
                tryDebuggingMethod.Syntax.WithBody(
                    Block(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("Console"), IdentifierName("WriteLine")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList<ArgumentSyntax>(
                                            Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("HELLO WORLD!")))))
                                )
                        ),
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                PredefinedType(
                                    Token(SyntaxKind.ObjectKeyword)
                                )
                            )
                            .WithVariables(
                                SingletonSeparatedList<VariableDeclaratorSyntax>(
                                    VariableDeclarator(
                                        Identifier("obj")
                                    )
                                    .WithInitializer(
                                        EqualsValueClause(
                                            LiteralExpression(
                                                SyntaxKind.NullLiteralExpression
                                            )
                                        )
                                    )
                                )
                            )
                        ),
                        LocalDeclarationStatement(
                            VariableDeclaration(
                                IdentifierName("var")
                            )
                            .WithVariables(
                                SingletonSeparatedList<VariableDeclaratorSyntax>(
                                    VariableDeclarator(
                                        Identifier("str")
                                    )
                                    .WithInitializer(
                                        EqualsValueClause(
                                            InvocationExpression(
                                                MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    IdentifierName("obj"),
                                                    IdentifierName("ToString")
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    ));
        }
    }
}
