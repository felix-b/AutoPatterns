using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns
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
