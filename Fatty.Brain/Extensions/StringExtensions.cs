namespace Fatty.Brain.Extensions
{
    using System;

    public static class StringExtensions
    {
        public static string SubstringToFirst(this string text, string delimiter)
        {
            return SubstringToFirst(text, delimiter, StringComparison.Ordinal);
        }
        
        public static string SubstringToFirst(this string text, string delimiter, StringComparison comparison)
        {
            if (text == null)
            {
                return null;
            }

            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            var firstIndex = text.IndexOf(delimiter, comparison);
            return firstIndex == -1 ? text : text.Substring(0, firstIndex);
        }

        public static string SubstringFromFirst(this string text, string delimiter)
        {
            return SubstringFromFirst(text, delimiter, StringComparison.Ordinal);
        }

        public static string SubstringFromFirst(this string text, string delimiter, StringComparison comparison)
        {
            if (text == null)
            {
                return null;
            }

            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            var firstIndex = text.IndexOf(delimiter, comparison);
            return firstIndex == -1 || firstIndex >= text.Length ? text : text.Substring(firstIndex + delimiter.Length);
        }
    }
}
