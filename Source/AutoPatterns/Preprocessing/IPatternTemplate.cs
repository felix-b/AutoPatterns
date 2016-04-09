using AutoPatterns.Runtime;

namespace AutoPatterns.Abstractions
{
    public interface IPatternTemplate
    {
        void Apply(PatternWriterContext context);
    }
}
