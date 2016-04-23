using AutoPatterns.Runtime;

namespace AutoPatterns.DesignTime
{
    public interface IPatternTemplate
    {
        void Apply(PatternWriterContext context);
    }
}
