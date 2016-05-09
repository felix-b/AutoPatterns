using System;

namespace AutoPatterns
{
    [Flags]
    public enum SelectOptions
    {
        None = 0,
        Implemented = 0x01,
        NotImplemented = 0x02,
        All = Implemented | NotImplemented
    }
}