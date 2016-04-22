using AutoPatterns.Extensions;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionTests
    {
        [TestCase("AStringWithSuffix", "AStringWith")]
        [TestCase("AStringWithout", "AStringWithout")]
        [TestCase("Suffix", "Suffix")]
        [TestCase("NotASuffixReally", "NotASuffixReally")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void TestTrimSuffix(string input, string expectedOutput)
        {
            //-- act

            var actualOutput = input.TrimSuffix("Suffix");

            //-- assert

            actualOutput.ShouldBe(expectedOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("PrefixAString", "AString")]
        [TestCase("StringWithout", "StringWithout")]
        [TestCase("Prefix", "Prefix")]
        [TestCase("NotAPrefixReally", "NotAPrefixReally")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void TestTrimPrefix(string input, string expectedOutput)
        {
            //-- act

            var actualOutput = input.TrimPrefix("Prefix");

            //-- assert

            actualOutput.ShouldBe(expectedOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("PascalCase", "pascalCase")]
        [TestCase("APascalCase", "aPascalCase")]
        [TestCase("DbPascalCase", "dbPascalCase")]
        [TestCase("UIPascalCase", "uiPascalCase")]
        [TestCase("HTMLPascalCase", "htmlPascalCase")]
        [TestCase("HTML", "html")]
        [TestCase("DB", "db")]
        [TestCase("Db", "db")]
        [TestCase("X", "x")]
        [TestCase("x", "x")]
        [TestCase("", "")]
        [TestCase(null, null)]
        public void TestToCamelCase(string input, string expectedOutput)
        {
            //-- act

            var actualOutput = input.ToCamelCase();

            //-- assert

            actualOutput.ShouldBe(expectedOutput);
        }
    }
}
