using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
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

            var library = TestLibrary.CreateLibraryWithDebug(assemblyName: this.GetType().Name);
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

            var library = TestLibrary.CreateLibraryWithDebug(assemblyName: this.GetType().Name);
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

        [Test, Explicit]
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

        [Test, Explicit]
        public void ExampleDebugger()
        {
            //-- arrange

            var library = new PatternLibrary(
                new InProcessPatternCompiler(), 
                assemblyName: this.GetType().Name, 
                assemblyDirectory: @"C:\Temp\TryDebugging", 
                enableDebug: true);

            var pattern = new TestPattern(library, pipeline => {
                pipeline.InsertLast(
                    new ExampleAutomaticProperty(), 
                    new ExampleDataContract(), 
                    new ExampleDebugger());
            });

            //-- act

            pattern.WriteExampleObject<ExampleAncestors.IScalarProperties>();

            var obj = pattern.CreateExampleObject<ExampleAncestors.IScalarProperties>();
            var debuggable = (ExampleAncestors.ITryDebugging)obj;

            obj.IntValue = 123;
            obj.StringValue = "ABC";
            obj.EnumValue = DayOfWeek.Thursday;
            obj.TimeSpanValue = TimeSpan.FromSeconds(123);

            debuggable.TryDebugging();

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.EnumValue.ShouldBe(DayOfWeek.Thursday);
            obj.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
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
