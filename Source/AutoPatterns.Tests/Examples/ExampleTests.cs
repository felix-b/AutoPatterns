using System;
using System.Diagnostics;
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
        public void CanGenerateObjectByAutomaticPropertyTemplate()
        {
            //-- arrange

            var library = new PatternLibrary(assemblyName: this.GetType().Name);
            var pattern = new ExampleAutomaticPropertyPattern(library);

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
                var clock = Stopwatch.StartNew();
                CanGenerateObjectByAutomaticPropertyTemplate();
                Console.WriteLine(clock.ElapsedMilliseconds);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PatternLibrary CreateTestLibrary()
        {
            return new PatternLibrary(this.GetType().Name, Assembly.GetExecutingAssembly());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExampleAutomaticPropertyPattern : AutoPattern
        {
            public ExampleAutomaticPropertyPattern(PatternLibrary library)
                : base(library)
            {
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
                pipeline.InsertLast(new ExampleAutomaticProperty());
            }
        }
    }
}
