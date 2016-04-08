using AutoPatterns.Runtime;

namespace AutoPatterns.Abstractions
{
    public interface IAutoPatternTemplate
    {
        void Apply(AutoPatternWriterContext context);
    }
}
