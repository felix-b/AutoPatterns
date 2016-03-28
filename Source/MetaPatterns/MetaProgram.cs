using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns
{
    public static partial class MetaProgram
    {
        public static MetaObject Object(object source)
        {
            return null;
        }

        public static T Proceed<T>(params object[] arguments)
        {
            return default(T);
        }

        public static void Proceed(params object[] arguments)
        {
        }

        public static MetaObject ThisObject { get; } = null;

        public static IDisposable TemplateLogic { get; } = null;
        public static IDisposable TemplateOutput { get; } = null;
    }
}
