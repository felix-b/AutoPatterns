#if false

using System;
using AutoPatterns;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MyNS
{
    [MetaProgram.Annotation.ClassTemplate]
    public partial class MyTemplate
    {
        [MetaProgram.Annotation.MetaMember(Repeat = RepeatOption.Once)]
        public void MyMethod()
        {
            var rand = new Random();
            var s = (rand.Next(0, 100) < 50 ? "A" : "B");
        }
    }

    [AutoPatterns.DesignTime.GeneratedTemplateImplementationAttribute(Hash = -835402933)]
    public partial class MyTemplate : IPatternTemplate
    {
        void IPatternTemplate.Apply(PatternWriterContext context)
        {
            context.Output.ClassWriter.AddMethod(MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("MyMethod")).WithAttributeLists(SingletonList<AttributeListSyntax>(AttributeList(SingletonSeparatedList<AttributeSyntax>(Attribute(QualifiedName(QualifiedName(IdentifierName("MetaProgram"), IdentifierName("Annotation")).WithDotToken(Token(SyntaxKind.DotToken)), IdentifierName("MetaMember")).WithDotToken(Token(SyntaxKind.DotToken))).WithArgumentList(AttributeArgumentList(SingletonSeparatedList<AttributeArgumentSyntax>(AttributeArgument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression(), IdentifierName("RepeatOption"), IdentifierName("Once")).WithOperatorToken(Token(SyntaxKind.DotToken))).WithNameEquals(NameEquals(IdentifierName("Repeat")).WithEqualsToken(Token(SyntaxKind.EqualsToken))))).WithOpenParenToken(Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(Token(SyntaxKind.CloseParenToken))))).WithOpenBracketToken(Token(SyntaxKind.OpenBracketToken)).WithCloseBracketToken(Token(SyntaxKind.CloseBracketToken)))).WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))).WithParameterList(ParameterList().WithOpenParenToken(Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(Token(SyntaxKind.CloseParenToken))).WithBody(Block(LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(Identifier("rand")).WithInitializer(EqualsValueClause(ObjectCreationExpression(IdentifierName("Random")).WithNewKeyword(Token(SyntaxKind.NewKeyword)).WithArgumentList(ArgumentList().WithOpenParenToken(Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(Token(SyntaxKind.CloseParenToken)))).WithEqualsToken(Token(SyntaxKind.EqualsToken)))))).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)), LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(VariableDeclarator(Identifier("s")).WithInitializer(EqualsValueClause(ParenthesizedExpression(ConditionalExpression(BinaryExpression(SyntaxKind.LessThanExpression(), InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression(), IdentifierName("rand"), IdentifierName("Next")).WithOperatorToken(Token(SyntaxKind.DotToken))).WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[](Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression(), Literal("0"))), Token(SyntaxKind.CommaToken), Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression(), Literal("100")))))).WithOpenParenToken(Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(Token(SyntaxKind.CloseParenToken))), LiteralExpression(SyntaxKind.NumericLiteralExpression(), Literal("50"))).WithOperatorToken(Token(SyntaxKind.LessThanToken)), LiteralExpression(SyntaxKind.StringLiteralExpression(), Literal("A")), LiteralExpression(SyntaxKind.StringLiteralExpression(), Literal("B"))).WithQuestionToken(Token(SyntaxKind.QuestionToken)).WithColonToken(Token(SyntaxKind.ColonToken))).WithOpenParenToken(Token(SyntaxKind.OpenParenToken)).WithCloseParenToken(Token(SyntaxKind.CloseParenToken))).WithEqualsToken(Token(SyntaxKind.EqualsToken)))))).WithSemicolonToken(Token(SyntaxKind.SemicolonToken))).WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken)).WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken))));
        }
    }
}

#endif