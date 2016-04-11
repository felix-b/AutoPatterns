using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoPatterns.Runtime;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.Examples
{
    [TestFixture]
    public class ExampleTests
    {
        [Test]
        public void ExampleAutomaticProperty()
        {
            //-- arrange

            var library = new PatternLibrary(assemblyName: this.GetType().Name);
            var pattern = new TestPattern(library, pipeline => {
                pipeline.InsertLast(new ExampleAutomaticProperty());                    
            });

            //-- act

            pattern.WriteExampleObject<ExampleAncestors.IScalarProperties>();

            var obj = pattern.CreateExampleObject<ExampleAncestors.IScalarProperties>();
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

        [Test]
        public void ExampleAutomaticPropertyAndDataContract()
        {
            //-- arrange

            var library = new PatternLibrary(assemblyName: this.GetType().Name);
            var pattern = new TestPattern(library, pipeline => {
                pipeline.InsertLast(new ExampleAutomaticProperty());
                pipeline.InsertLast(new ExampleDataContract());
            });

            //-- act

            pattern.WriteExampleObject<ExampleAncestors.IScalarProperties>();

            var obj = pattern.CreateExampleObject<ExampleAncestors.IScalarProperties>();
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

        [Test]
        public void BenchmarkGenerateObjectByAutomaticPropertyTemplate()
        {
            for (int i = 0 ; i < 50 ; i++)
            {
                try
                {
                    //ExampleAutomaticProperty();
                    ExampleAutomaticPropertyAndDataContract();
                }
                catch {  }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PatternLibrary CreateTestLibrary()
        {
            return new PatternLibrary(this.GetType().Name, Assembly.GetExecutingAssembly());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestPattern : AutoPattern
        {
            private readonly Action<PipelineBuilder> _onBuildPipeline;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestPattern(PatternLibrary library, Action<PipelineBuilder> onBuildPipeline)
                : base(library)
            {
                _onBuildPipeline = onBuildPipeline;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void WriteExampleObject<T>()
            {
                Writer.EnsureWritten(new TypeKey<Type>(typeof(T)), primaryInterface: typeof(T));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T CreateExampleObject<T>()
            {
                return (T)Factory.CreateInstance(new TypeKey<Type>(typeof(T)), constructorIndex: 0);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void BuildPipeline(PatternWriterContext context, PipelineBuilder pipeline)
            {
                _onBuildPipeline(pipeline);
            }
        }
    }
}
