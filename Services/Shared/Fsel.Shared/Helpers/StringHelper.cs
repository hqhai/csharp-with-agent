// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Globalization;
    using System.Text.RegularExpressions;

    public static class StringHelper
    {
        public static string RemoveHTMLTags(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        public static bool ContainsIgnoreCase(this string? str, string value)
        {
            if (str == null)
            {
                return false;
            }

            return str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string ToNormalizedString(this string? str)
        {
            return (str ?? string.Empty).ToLower().Trim();
        }
    }
}
