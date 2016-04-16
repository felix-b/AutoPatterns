using System;

namespace AutoPatterns
{
    [Flags]
    public enum SelectOptions
    {
        Implemented = 0x01,
        NotImplemented = 0x02,
        All = Implemented | NotImplemented
    }
}