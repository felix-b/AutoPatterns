using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaPatterns.Tests.Examples;
using MetaPatterns.Tests.Repo;
using NUnit.Framework;
using Shouldly;

namespace MetaPatterns.Tests
{
    [TestFixture]
    public class ExampleTests
    {
        [Test]
        public void CanGenerateObjectByAutomaticPropertyTemplate()
        {
            //-- arrange

            var factory = new ExampleAutomaticPropertyFactory(new Net45MetaPatternsPlatform());

            //-- act

            IHaveScalarProperties obj = factory.CreateInstance<IHaveScalarProperties>();
            obj.IntValue = 123;
            obj.StringValue = "ABC";
            obj.EnumValue = DayOfWeek.Thursday;
            obj.TimeSpanValue = TimeSpan.FromSeconds(123);

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.EnumValue.ShouldBe(DayOfWeek.Thursday);
            obj.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
        }
    }
}
