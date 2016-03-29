using System;
using System.Collections.Generic;
using System.Reflection;
using MetaPatterns.Abstractions;
using MetaPatterns.Tests.Repo;
using NUnit.Framework;
using Shouldly;

namespace MetaPatterns.Tests.Examples
{
    [TestFixture]
    public class ExampleTests
    {
        [Test]
        public void CanGenerateObjectByAutomaticPropertyTemplate()
        {
            //-- arrange

            var compiler = new ExampleAutomaticPropertyCompiler(new Net45MetaPatternsPlatform());
            compiler.CompileExampleObject();

            var assembly = MetaPatternCompiler.CompileAndLoadAssembly(new MetaPatternCompiler[] { compiler });
            var factory = new ExampleAutomaticPropertyFactory(assembly);

            dynamic obj = factory.CreateExampleObject();
            obj.IntValue = 123;
            int value = obj.IntValue;
            value.ShouldBe(123);

            ////-- act

            //IHaveScalarProperties obj = factory.CreateExampleObject();
            //obj.IntValue = 123;
            //obj.StringValue = "ABC";
            //obj.EnumValue = DayOfWeek.Thursday;
            //obj.TimeSpanValue = TimeSpan.FromSeconds(123);

            ////-- assert

            //obj.IntValue.ShouldBe(123);
            //obj.StringValue.ShouldBe("ABC");
            //obj.EnumValue.ShouldBe(DayOfWeek.Thursday);
            //obj.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAutomaticPropertyCompiler : MetaPatternCompiler
        {
            public ExampleAutomaticPropertyCompiler(IMetaPatternCompilerPlatform platform)
                : base(platform)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void CompileExampleObject()
            {
                BuildSyntax(new TypeKey<int>(123));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override IMetaPatternTemplate[] BuildPipeline(MetaPatternCompilerContext context)
            {
                return new IMetaPatternTemplate[] {
                    new ExampleAutomaticProperty()
                };
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAutomaticPropertyFactory : MetaPatternFactory
        {
            public ExampleAutomaticPropertyFactory(params Assembly[] assemblies)
                : base(assemblies)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object CreateExampleObject()
            {
                return base.CreateInstance(new TypeKey<int>(123), constructorIndex: 0);
            }
        }
    }
}
