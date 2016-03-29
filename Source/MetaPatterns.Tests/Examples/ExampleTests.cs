using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MetaPatterns.Abstractions;
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
            compiler.CompileExampleObject<TestTypes.IScalarProperties>();

            var assemblyBytes = MetaPatternCompiler.CompileAssembly(new MetaPatternCompiler[] { compiler }, "EmittedByExampleTests");
            File.WriteAllBytes($@"C:\Temp\EmittedByExampleTests.dll", assemblyBytes);
            var assembly = Assembly.Load(assemblyBytes); //MetaPatternCompiler.CompileAndLoadAssembly(new MetaPatternCompiler[] { compiler });
            var factory = new ExampleAutomaticPropertyFactory(assembly);

            //-- act

            var obj = factory.CreateExampleObject<TestTypes.IScalarProperties>();
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAutomaticPropertyCompiler : MetaPatternCompiler
        {
            public ExampleAutomaticPropertyCompiler(IMetaPatternCompilerPlatform platform)
                : base(platform)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void CompileExampleObject<T>()
            {
                BuildSyntax(new TypeKey<Type>(typeof(T)), primaryInterface: typeof(T));
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

            public T CreateExampleObject<T>()
            {
                return (T)base.CreateInstance(new TypeKey<Type>(typeof(T)), constructorIndex: 0);
            }
        }
    }
}

namespace ExampleAutomaticProperty
{
    using System;

    public class MetaPatterns_Tests_TestTypes_IScalarProperties : MetaPatterns.Tests.TestTypes.IScalarProperties
    {
        public static object FactoryMethod__0()
        {
            return new MetaPatterns_Tests_TestTypes_IScalarProperties();
        }

        public System.Int32 IntValue
        {
            get;
            set;
        }

        public System.String StringValue
        {
            get;
            set;
        }

        public System.DayOfWeek EnumValue
        {
            get;
            set;
        }

        public System.TimeSpan TimeSpanValue
        {
            get;
            set;
        }
    }
}