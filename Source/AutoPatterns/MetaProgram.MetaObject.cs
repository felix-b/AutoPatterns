using System.Collections.Generic;
using System.Reflection;

namespace AutoPatterns
{
    public static partial class MetaProgram
    {
        public class MetaObject
        {
            public MetaField Field(MetaField sameAs)
            {
                return null;
            }

            public IReadOnlyList<MetaField> Fields { get; } = new List<MetaField>();

            public class MetaField
            {
                public object Value { get; } = null;
                public FieldInfo Info { get; } = null;
            }
        }
    }
}
