namespace AutoPatterns.Extensions
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            int firstLowerCaseIndex;

            for (firstLowerCaseIndex = 0 ; firstLowerCaseIndex < str.Length ; firstLowerCaseIndex++)
            {
                if (char.IsLower(str[firstLowerCaseIndex]))
                {
                    break;
                }
            }

            if (firstLowerCaseIndex == 0)
            {
                return str;
            }

            if (firstLowerCaseIndex == 1)
            {
                return char.ToLower(str[0]) + str.Substring(1);
            }

            if (firstLowerCaseIndex >= str.Length)
            {
                return str.ToLower();
            }

            return str.Substring(0, firstLowerCaseIndex - 1).ToLower() + str.Substring(firstLowerCaseIndex - 1);
        }
    }
}
