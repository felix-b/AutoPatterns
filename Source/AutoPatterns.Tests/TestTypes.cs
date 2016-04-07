using System;

namespace AutoPatterns.Tests
{
    public static class TestTypes
    {
        public interface IScalarProperties
        {
            int IntValue { get; set; }
            string StringValue { get; set; }
            DayOfWeek EnumValue { get; set; }
            TimeSpan TimeSpanValue { get; set; }
        }
    }
}
