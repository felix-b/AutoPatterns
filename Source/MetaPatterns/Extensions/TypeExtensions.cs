using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns.Extensions
{
    public static class TypeExtensions
    {
        public static Type[] OrEmptyTypes(this Type[] types)
        {
            return (types ?? EmptyTypes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Type[] EmptyTypes { get; } = new Type[0];
    }
}
