using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoPatterns.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;
using Shouldly;

namespace AutoPatterns.Tests.Runtime
{
    [TestFixture]
    public class PatternFactoryTests
    {
        [Test]
        public void CreateObject_ParameterlessConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var obj = pattern.CreateTestClassA();

            //-- assert

            obj.IntValue.ShouldBe(0);
            obj.StringValue.ShouldBe(null);
            obj.DecimalValue.ShouldBe(0m);
            obj.EnumValue.ShouldBe((DayOfWeek)0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_1ArgumentConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var obj = pattern.CreateTestClassA(123);

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe(null);
            obj.DecimalValue.ShouldBe(0m);
            obj.EnumValue.ShouldBe((DayOfWeek)0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_2ArgumentsConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var obj = pattern.CreateTestClassA(123, "ABC");

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.DecimalValue.ShouldBe(0m);
            obj.EnumValue.ShouldBe((DayOfWeek)0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_3ArgumentsConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var obj = pattern.CreateTestClassA(123, "ABC", 123.45m);

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.DecimalValue.ShouldBe(123.45m);
            obj.EnumValue.ShouldBe((DayOfWeek)0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_4ArgumentsConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var obj = pattern.CreateTestClassA(123, "ABC", 123.45m, DayOfWeek.Tuesday);

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.DecimalValue.ShouldBe(123.45m);
            obj.EnumValue.ShouldBe(DayOfWeek.Tuesday);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_5ArgumentsConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var obj = pattern.CreateTestClassB(123, "ABC", 123.45m, DayOfWeek.Tuesday, TimeSpan.FromSeconds(123));

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.DecimalValue.ShouldBe(123.45m);
            obj.EnumValue.ShouldBe(DayOfWeek.Tuesday);
            obj.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
            obj.DateTimeValue.ShouldBe(default(DateTime));
            obj.StopwatchValue.ShouldBe(null);
            obj.ByteArrayValue.ShouldBe(null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_6ArgumentsConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var obj = pattern.CreateTestClassB(123, "ABC", 123.45m, DayOfWeek.Tuesday, TimeSpan.FromSeconds(123), new DateTime(2010, 10, 10));

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.DecimalValue.ShouldBe(123.45m);
            obj.EnumValue.ShouldBe(DayOfWeek.Tuesday);
            obj.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
            obj.DateTimeValue.ShouldBe(new DateTime(2010, 10, 10));
            obj.StopwatchValue.ShouldBe(null);
            obj.ByteArrayValue.ShouldBe(null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_7ArgumentsConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());
            var stopwatch = Stopwatch.StartNew();

            //-- act

            var obj = pattern.CreateTestClassB(123, "ABC", 123.45m, DayOfWeek.Tuesday, TimeSpan.FromSeconds(123), new DateTime(2010, 10, 10), stopwatch);

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.DecimalValue.ShouldBe(123.45m);
            obj.EnumValue.ShouldBe(DayOfWeek.Tuesday);
            obj.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
            obj.DateTimeValue.ShouldBe(new DateTime(2010, 10, 10));
            obj.StopwatchValue.ShouldBeSameAs(stopwatch);
            obj.ByteArrayValue.ShouldBe(null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CreateObject_8ArgumentsConstructor()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());
            var stopwatch = Stopwatch.StartNew();
            var byteArray = new byte[] { 1, 2, 3 };

            //-- act

            var obj = pattern.CreateTestClassB(123, "ABC", 123.45m, DayOfWeek.Tuesday, TimeSpan.FromSeconds(123), new DateTime(2010, 10, 10), stopwatch, byteArray);

            //-- assert

            obj.IntValue.ShouldBe(123);
            obj.StringValue.ShouldBe("ABC");
            obj.DecimalValue.ShouldBe(123.45m);
            obj.EnumValue.ShouldBe(DayOfWeek.Tuesday);
            obj.TimeSpanValue.ShouldBe(TimeSpan.FromSeconds(123));
            obj.DateTimeValue.ShouldBe(new DateTime(2010, 10, 10));
            obj.StopwatchValue.ShouldBeSameAs(stopwatch);
            obj.ByteArrayValue.ShouldBeSameAs(byteArray);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void EachTypeEntryIsOnlyCreatedOnce()
        {
            //-- arrange

            var pattern = new TestPatternOne(CreateTestLibrary());

            //-- act

            var log0 = pattern.TypeEntryLog.ToArray();

            pattern.CreateTestClassA();

            var log1 = pattern.TypeEntryLog.ToArray();

            pattern.CreateTestClassB(123, "ABC", 0.5m, DayOfWeek.Friday, TimeSpan.Zero);

            var log2 = pattern.TypeEntryLog.ToArray();

            pattern.CreateTestClassA();
            pattern.CreateTestClassA();
            pattern.CreateTestClassB(123, "ABC", 0.5m, DayOfWeek.Friday, TimeSpan.Zero);
            pattern.CreateTestClassB(123, "ABC", 0.5m, DayOfWeek.Friday, TimeSpan.Zero);

            var log3 = pattern.TypeEntryLog.ToArray();

            //-- assert

            log0.ShouldBeEmpty();
            log1.ShouldBe(new[] { typeof(TestClassA) });
            log2.ShouldBe(new[] { typeof(TestClassA), typeof(TestClassB) });
            log3.ShouldBe(new[] { typeof(TestClassA), typeof(TestClassB) });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanUseAssemblyGeneratedWithRoslyn()
        {
            //-- arrange

            var syntaxTree = CSharpSyntaxTree.ParseText(@"
                namespace MyNamespace {
                    public class MyClass {
                        public MyClass(int value) { this.IntValue = value; }
                        public int IntValue { get; private set; }
                        public static object FactoryMethod__0(int value) { return new MyClass(value); }
                    }
                }
            ");

            var compilation = CSharpCompilation
                .Create("MyTest", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(syntaxTree);

            Assembly generatedAssembly;

            using (var output = new MemoryStream(capacity: 16384))
            {
                EmitResult result = compilation.Emit(output);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    throw new Exception(
                        "Compile failed:" +
                        System.Environment.NewLine +
                        string.Join(System.Environment.NewLine, failures.Select(f => $"{f.Id}: {f.GetMessage()}")));
                }

                generatedAssembly = Assembly.Load(output.ToArray());
            }

            var library = new PatternLibrary(assemblyName: "NotUsed", preloadedAssemblies: generatedAssembly);
            var factory = new TestPatternTwo(library);

            //-- act

            var obj = factory.CreateMyClass(123);

            //-- assert

            obj.ShouldNotBeNull();
            obj.GetType().Assembly.ShouldBeSameAs(generatedAssembly);
            obj.GetType().FullName.ShouldBe("MyNamespace.MyClass");

            dynamic dyn = obj;
            int intValue = dyn.IntValue;
            intValue.ShouldBe(123);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PatternLibrary CreateTestLibrary()
        {
            return new PatternLibrary(this.GetType().Name, Assembly.GetExecutingAssembly());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestPatternOne : AutoPattern
        {
            private readonly List<Type> _typeEntryLog = new List<Type>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestPatternOne(PatternLibrary library)
                : base(library, namespaceName: "")
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestClassA CreateTestClassA()
            {
                return (TestClassA)Factory.CreateInstance(new TypeKey<Type>(typeof(TestClassA)), 0);
            }
            public TestClassA CreateTestClassA(int intValue)
            {
                return (TestClassA)Factory.CreateInstance<int>(new TypeKey<Type>(typeof(TestClassA)), 1, intValue);
            }
            public TestClassA CreateTestClassA(int intValue, string stringValue)
            {
                return (TestClassA)Factory.CreateInstance<int, string>(new TypeKey<Type>(typeof(TestClassA)), 2, intValue, stringValue);
            }
            public TestClassA CreateTestClassA(int intValue, string stringValue, decimal decimalValue)
            {
                return (TestClassA)Factory.CreateInstance<int, string, decimal>(new TypeKey<Type>(typeof(TestClassA)), 3, intValue, stringValue, decimalValue);
            }
            public TestClassA CreateTestClassA(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue)
            {
                return (TestClassA)Factory.CreateInstance<int, string, decimal, DayOfWeek>(
                    new TypeKey<Type>(typeof(TestClassA)), 4, intValue, stringValue, decimalValue, enumValue);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TestClassB CreateTestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue)
            {
                return (TestClassB)Factory.CreateInstance<int, string, decimal, DayOfWeek, TimeSpan>(
                    new TypeKey<Type>(typeof(TestClassB)), 
                    0, 
                    intValue, stringValue, decimalValue, enumValue, timeSpanValue);
            }
            public TestClassB CreateTestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue)
            {
                return (TestClassB)Factory.CreateInstance<int, string, decimal, DayOfWeek, TimeSpan, DateTime>(
                    new TypeKey<Type>(typeof(TestClassB)),
                    1,
                    intValue, stringValue, decimalValue, enumValue, timeSpanValue, dateTimeValue);
            }
            public TestClassB CreateTestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue, Stopwatch stopwatchValue)
            {
                return (TestClassB)Factory.CreateInstance<int, string, decimal, DayOfWeek, TimeSpan, DateTime, Stopwatch>(
                    new TypeKey<Type>(typeof(TestClassB)),
                    2,
                    intValue, stringValue, decimalValue, enumValue, timeSpanValue, dateTimeValue, stopwatchValue);
            }
            public TestClassB CreateTestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue, Stopwatch stopwatchValue, byte[] byteArrayValue)
            {
                return (TestClassB)Factory.CreateInstance<int, string, decimal, DayOfWeek, TimeSpan, DateTime, Stopwatch, byte[]>(
                    new TypeKey<Type>(typeof(TestClassB)),
                    3,
                    intValue, stringValue, decimalValue, enumValue, timeSpanValue, dateTimeValue, stopwatchValue, byteArrayValue);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IReadOnlyList<Type> TypeEntryLog => _typeEntryLog;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override string GetClassName(TypeKey key)
            {
                return key[0].ToString();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void BuildPipeline(PatternWriterContext context, PipelineBuilder pipeline)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected internal override void OnTypeBound(PatternFactory.TypeEntry entry)
            {
                _typeEntryLog.Add(entry.Type);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestPatternTwo : AutoPattern
        {
            public TestPatternTwo(PatternLibrary library)
                : base(library, namespaceName: "MyNamespace")
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object CreateMyClass(int value)
            {
                return Factory.CreateInstance<int>(new TypeKey<string>("MyClass"), constructorIndex: 0, arg1: value);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void BuildPipeline(PatternWriterContext context, PipelineBuilder pipeline)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestClassA
        {
            private TestClassA()
            {
            }
            private TestClassA(int intValue)
            {
                IntValue = intValue;
            }
            private TestClassA(int intValue, string stringValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
            }
            private TestClassA(int intValue, string stringValue, decimal decimalValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                DecimalValue = decimalValue;
            }
            private TestClassA(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                DecimalValue = decimalValue;
                EnumValue = enumValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int IntValue { get; private set; }
            public string StringValue { get; private set; }
            public decimal DecimalValue { get; private set; }
            public DayOfWeek EnumValue { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static object FactoryMethod__0()
            {
                return new TestClassA();
            }
            public static object FactoryMethod__1(int intValue)
            {
                return new TestClassA(intValue);
            }
            public static object FactoryMethod__2(int intValue, string stringValue)
            {
                return new TestClassA(intValue, stringValue);
            }
            public static object FactoryMethod__3(int intValue, string stringValue, decimal decimalValue)
            {
                return new TestClassA(intValue, stringValue, decimalValue);
            }
            public static object FactoryMethod__4(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue)
            {
                return new TestClassA(intValue, stringValue, decimalValue, enumValue);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestClassB
        {
            public TestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                DecimalValue = decimalValue;
                EnumValue = enumValue;
                TimeSpanValue = timeSpanValue;
            }
            public TestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                DecimalValue = decimalValue;
                EnumValue = enumValue;
                TimeSpanValue = timeSpanValue;
                DateTimeValue = dateTimeValue;
            }
            public TestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue, Stopwatch stopwatchValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                DecimalValue = decimalValue;
                EnumValue = enumValue;
                TimeSpanValue = timeSpanValue;
                DateTimeValue = dateTimeValue;
                StopwatchValue = stopwatchValue;
            }
            public TestClassB(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue, Stopwatch stopwatchValue, byte[] byteArrayValue)
            {
                IntValue = intValue;
                StringValue = stringValue;
                DecimalValue = decimalValue;
                EnumValue = enumValue;
                TimeSpanValue = timeSpanValue;
                DateTimeValue = dateTimeValue;
                StopwatchValue = stopwatchValue;
                ByteArrayValue = byteArrayValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int IntValue { get; private set; }
            public string StringValue { get; private set; }
            public decimal DecimalValue { get; private set; }
            public DayOfWeek EnumValue { get; private set; }
            public TimeSpan TimeSpanValue { get; private set; }
            public DateTime DateTimeValue { get; private set; }
            public Stopwatch StopwatchValue { get; private set; }
            public byte[] ByteArrayValue { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static object FactoryMethod__0(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue)
            {
                return new TestClassB(intValue, stringValue, decimalValue, enumValue, timeSpanValue);
            }
            public static object FactoryMethod__1(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue)
            {
                return new TestClassB(intValue, stringValue, decimalValue, enumValue, timeSpanValue, dateTimeValue);
            }
            public static object FactoryMethod__2(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue, Stopwatch stopwatchValue)
            {
                return new TestClassB(intValue, stringValue, decimalValue, enumValue, timeSpanValue, dateTimeValue, stopwatchValue);
            }
            public static object FactoryMethod__3(int intValue, string stringValue, decimal decimalValue, DayOfWeek enumValue, TimeSpan timeSpanValue, DateTime dateTimeValue, Stopwatch stopwatchValue, byte[] byteArrayValue)
            {
                return new TestClassB(intValue, stringValue, decimalValue, enumValue, timeSpanValue, dateTimeValue, stopwatchValue, byteArrayValue);
            }
        }
    }
}
