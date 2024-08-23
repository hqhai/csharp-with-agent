// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringHelper
    {
        public static string RemoveWhitespace(string input)
        {
            StringBuilder sb = new StringBuilder();
            using (StringReader sr = new StringReader(input))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    sb.Append(line.Trim());
                }
            }
            return sb.ToString();
        }

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

        public static string ShuffleCharactersWithinWords(string str)
        {
            char[] characters = str.ToCharArray();
            Random rng = new Random();

            int n = characters.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                char temp = characters[k];
                characters[k] = characters[n];
                characters[n] = temp;
            }
            return new string(characters);
        }

        public static string RandomCharacters(string str)
        {
            string[] words = str.Split(' ');
            StringBuilder result = new StringBuilder();

            foreach (string word in words)
            {
                string shuffledWord = ShuffleCharactersWithinWords(word);
                int count = 0;
                while (count < 3)
                {
                    shuffledWord = ShuffleCharactersWithinWords(word);
                    count++;
                }

                result.Append(shuffledWord);
                result.Append(' ');
            }

            result.Length--;

            return result.ToString();
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
                else
                {
                    // Nếu không phải 'ql-audio' và cũng không phải 'ql-video', thì xóa thẻ iframe
                    return string.Empty;
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

        public static int CountWords(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return 0;
            }
            var words = input.StringSplitToList();
            return words.Count;
        }

        public static IList<string> StringSplitToList(this string input)
        {
            return input?.TrimHiddenChars().ToLower(CultureInfo.CurrentCulture).Split(new[] { " ", "\n", "\r", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? new List<string>();
        }

        public static string ReplaceWord(this string? word)
        {
            string pattern = "[‘'’ʼ]";
            string replacement = "'";
            return word?.TrimHiddenChars().ToLower(CultureInfo.CurrentCulture).ReplaceWord(pattern, replacement) ?? string.Empty;
        }

        public static string TrimHiddenChars(this string? word)
        {
            return word?.Trim(new char[] { ' ', '​', '\t', '.', '\n', '\r' }) ?? string.Empty;
        }

        public static string ReplaceWord(this string? word, string pattern, string replacement)
        {
            return Regex.Replace(word ?? string.Empty, pattern, replacement);
        }

        public static bool IsBase64Image(string? inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return false;
            }
            // Mẫu để khớp với URI dữ liệu của một hình ảnh có nội dung Base64
            string base64Pattern = @"^data:image\/(jpeg|jpg|png|gif|bmp|tiff);base64,([A-Za-z0-9+/]+={0,2})$";
            return Regex.IsMatch(inputString, base64Pattern, RegexOptions.Compiled);
        }
    }
}
