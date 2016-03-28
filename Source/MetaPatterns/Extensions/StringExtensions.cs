using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaPatterns.Extensions
{
    public static class StringExtensions
    {
        public static string TrimPrefix(this string str, string prefix)
        {
            if (str != null && prefix != null && str.StartsWith(prefix) && str.Length > prefix.Length)
            {
                return str.Substring(prefix.Length);
            }
            else
            {
                return str;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string TrimSuffix(this string str, string suffix)
        {
            if (str != null && suffix != null && str.EndsWith(suffix) && str.Length > suffix.Length)
            {
                return str.Substring(0, str.Length - suffix.Length);
            }
            else
            {
                return str;
            }
        }
    }
}
