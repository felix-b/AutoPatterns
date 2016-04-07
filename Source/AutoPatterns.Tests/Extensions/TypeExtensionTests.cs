using System;
using System.Collections.Generic;
using System.Reflection;
using AutoPatterns.Extensions;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.Extensions
{
    [TestFixture]
    public class TypeExtensionTests
    {
        private readonly static object[] _s_testFriendlyNameCases = {
            new object[] { typeof(string), "String" },
            new object[] { typeof(string[]), "String[]" },
            new object[] { typeof(List<string>), "List<String>" },
            new object[] { typeof(Dictionary<int, string>), "Dictionary<Int32,String>" },
            new object[] { typeof(Dictionary<List<DayOfWeek>, List<Dictionary<int, string>>>), "Dictionary<List<DayOfWeek>,List<Dictionary<Int32,String>>>" },
            new object[] { typeof(AnOuterNonGenericType.AnInnerType), "TypeExtensionTests.AnOuterNonGenericType.AnInnerType" },
            new object[] { typeof(AnOuterGenericType<string>.AnInnerType), "TypeExtensionTests.AnOuterGenericType<String>.AnInnerType" },
            new object[] { typeof(AnOuterGenericType<string>.AGenericInnerType<int>), "TypeExtensionTests.AnOuterGenericType<String>.AGenericInnerType<Int32>" }
        };
        [TestCaseSource(nameof(_s_testFriendlyNameCases))]
        public void FriendlyName_DefaultPunctuation(Type type, string expectedFriendlyName)
        {
            //-- act

            var actualFriendlyName = type.GetTypeInfo().FriendlyName();

            //-- assert

            actualFriendlyName.ShouldBe(expectedFriendlyName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void FriendlyName_CustomPunctuation()
        {
            //-- arrange

            var type = typeof(Dictionary<int, AnOuterNonGenericType.AnInnerType>);
            var expectedFriendlyName = "Dictionary{Int32|TypeExtensionTests:AnOuterNonGenericType:AnInnerType}";
            
            //-- act

            var actualFriendlyName = type.GetTypeInfo().FriendlyName(punctuation: ":`{|}");

            //-- assert

            actualFriendlyName.ShouldBe(expectedFriendlyName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AnOuterNonGenericType
        {
            public class AnInnerType
            {
            }
            public class AGenericInnerType<T>
            {
            }
        }
        public class AnOuterGenericType<T>
        {
            public class AnInnerType
            {
            }
            public class AGenericInnerType<S>
            {
            }
        }
    }
}
