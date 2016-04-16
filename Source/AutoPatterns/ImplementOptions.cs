using System;

namespace AutoPatterns
{
    [Flags]
    public enum ImplementOptions
    {
        None = 0,
        Attributes = 0x01,
        Body = 0x02,
        All = Attributes | Body
    }
}