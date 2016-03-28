using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaPatterns.Extensions;
using NUnit.Framework;
using Shouldly;

namespace MetaPatterns.Tests.Extensions
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
    }
}
