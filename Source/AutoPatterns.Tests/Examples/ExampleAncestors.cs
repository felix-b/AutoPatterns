using System;

namespace AutoPatterns.Tests.Examples
{
    public static class ExampleAncestors
    {
        public interface IScalarProperties
        {
            int IntValue { get; set; }
            string StringValue { get; set; }
            DayOfWeek EnumValue { get; set; }
            TimeSpan TimeSpanValue { get; set; }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------------------

        public interface ITryDebugging
        {
            void TryDebugging();
        }
    }
}
