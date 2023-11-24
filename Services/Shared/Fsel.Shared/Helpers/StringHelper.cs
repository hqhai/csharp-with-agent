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

        public static string ProcessHtml(string? inputHtml, bool indexAudioOnly)
        {
            if (string.IsNullOrEmpty(inputHtml))
            {
                return string.Empty;
            }

            // Regex để tìm các thẻ iframe có class là 'ql-audio' hoặc 'ql-video'
            string pattern = @"<iframe\s+class=""(ql-audio|ql-video)""[^>]*>.*?<\/iframe>";

            // Hàm thay thế các thẻ iframe bằng placeholders
            int audioIndex = 1;
            string result = Regex.Replace(inputHtml, pattern, match =>
            {
                string iframeClass = match.Groups[1].Value;

                if (iframeClass == "ql-audio" && indexAudioOnly)
                {
                    // Nếu indexAudioOnly là true và iframe là 'ql-audio', thì thêm placeholder với index audio
                    string placeholder = $"<audio>{audioIndex}</audio>";
                    audioIndex++;
                    return placeholder;
                }
                else if (iframeClass == "ql-video" && !indexAudioOnly)
                {
                    // Nếu iframe là 'ql-video' và indexAudioOnly là false, thì xóa nó
                    return "";
                }
                else
                {
                    // Nếu không phải 'ql-audio' và cũng không phải 'ql-video', thì xóa thẻ iframe
                    return "";
                }
            }, RegexOptions.Singleline);

            return result;
        }

        public static IEnumerable<string>? GetIframeUrls(string? inputHtml, bool isAudio)
        {
            if (string.IsNullOrEmpty(inputHtml))
            {
                return null;
            }

            List<string>? urls = new List<string>();

            // Xác định class cần tìm
            string targetClass = isAudio ? "ql-audio" : "ql-video";

            // Regex để tìm các thẻ iframe có class là 'ql-audio' hoặc 'ql-video'
            string pattern = $@"<iframe\s+class=""{targetClass}""[^>]*\ssrc=""([^""]+)""[^>]*>.*?<\/iframe>";

            MatchCollection matches = Regex.Matches(inputHtml, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                string url = match.Groups[1].Value;
                urls.Add(url);
            }

            return urls;
        }
    }
}
