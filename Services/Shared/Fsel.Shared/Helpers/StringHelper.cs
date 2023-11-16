// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.ComponentModel;
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

            return str.Contains(value, StringComparison.OrdinalIgnoreCase);
        }

        public static string? ToNormalizedString(this string? str)
        {
            return (str ?? string.Empty).ToLower(CultureInfo.CurrentCulture).Trim();
        }

        public static IList<T>? ToList<T>(this string? str, char separator = ',')
        {
            return str?.Split(separator).Select(x =>
            {
                if (TypeDescriptor.GetConverter(typeof(T)).IsValid(x))
                {
                    return (T?)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(x) ?? default;
                }
                return default;
            }).Where(x => x != null).Select(x => x!).ToList();
        }

        public static IList<T> ToList<T>(this IEnumerable<string>? values, char separator = ',')
        {
            var results = new List<T>();
            if (values == null)
            {
                return results;
            }

            foreach (var value in values)
            {
                var datas = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                foreach (var data in datas)
                {
                    if (TypeDescriptor.GetConverter(typeof(T)).IsValid(data))
                    {
                        var result = (T?)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(data);
                        if (result != null)
                        {
                            results.Add(result);
                        }
                    }
                }
            }
            return results;
        }
    }
}
