using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.DesignTime
{
    [TestFixture]
    public class QuoterTests
    {
        [Test]
        public void CanBuildSyntaxNodeFromApiCall()
        {
            //-- arrange

            var code = @"
                public void MyMethod()
                {
                    var rand = new Random();
                    var s = (rand.Next(0, 100) < 50 ? ""A"" : ""B"");
                }
            ";

            var quoter = new Quoter();
            var parsedSyntax = CSharpSyntaxTree.ParseText(code).GetRoot().NormalizeWhitespace();

            //-- act

            var actualSyntax = quoter.Quote(parsedSyntax, name: null).ToSyntaxNode().NormalizeWhitespace();

            //-- assert

            var expectedSyntaxText = parsedSyntax.ToFullString();
            var actualSyntaxText = actualSyntax.ToFullString();

            actualSyntaxText.ShouldBe(expectedSyntaxText);
        }
    }
}
