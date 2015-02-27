using System;
using System.Text.RegularExpressions;

namespace Sentinel.Support
{
    public static class RegexExtensions
    {
        public static bool IsRegexMatch(this string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) throw new ArgumentNullException("pattern");
            return Regex.IsMatch(source, pattern);
        }
    }
}
