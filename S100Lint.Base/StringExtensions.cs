using System;
using System.Globalization;

namespace S100Lint.Base
{
    public static class StringExtensions
    {
        public static string UncapitalizeFirst(this string item)
        {
            if (item.Length == 0)
            {
                return String.Empty;
            }

            if (item.Length == 1)
            {
                return item.ToLower();
            }

            return $"{item[0].ToString().ToLower()}{item.Substring(1)}";
        }

        public static string LastPart(this string item, char startFrom)
        {
            var start = item.LastIndexOf(startFrom);
            if (start <= 0)
            {
                return item;
            }

            return item.Substring(start + 1, item.Length - start - 1);
        }

        public static string LastPart(this string item, string startFrom)
        {
            var start = item.LastIndexOf(char.Parse(startFrom));
            if (start <= 0)
            {
                return item;
            }

            return item.Substring(start + 1, item.Length - start - 1);
        }

    }
}
