// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Web;
    using Fsel.Shared.Constants;
    using Nest;

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
            values = values?.Where(x => x != null).ToList();
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
            string replacement = "'";
            return word?.TrimHiddenChars().ToLower(CultureInfo.CurrentCulture).ReplaceWord(RegexSetting.WordPattern, replacement) ?? string.Empty;
        }

        public static string TrimHiddenChars(this string? word)
        {
            return word?.Trim(new char[] { ' ', '​', '\t', '.', '\n', '\r' }) ?? string.Empty;
        }

        public static string ReplaceWord(this string? word, string pattern, string replacement)
        {
            return Regex.Replace(word ?? string.Empty, pattern, replacement);
        }

        public static bool ContainsSpecialCharacter(string input)
        {
            Regex regex = new Regex(RegexSetting.SpecialCharacterPattern);
            return regex.IsMatch(input);
        }

        public static ICollection<string> GetEnumNames<T>() where T : Enum
        {
            return new List<string>(Enum.GetNames(typeof(T)));
        }

        public static string RemoveMarkdownFromJson(string json)
        {
            string cleanedJson = Regex.Replace(json, RegexSetting.AiReponseJsonPattern, "");
            return cleanedJson;
        }

        public static bool IsBase64Image(string? inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return false;
            }
            return Regex.IsMatch(inputString, RegexSetting.Base64Pattern, RegexOptions.Compiled);
        }

        //Kiểm tra string có chứa khoảng trắng hay kí tự đặc biệt không
        public static bool ContainsWhitespaceOrSpecialChars(string input)
        {
            return Regex.IsMatch(input, "^[a-zA-Z0-9]+$");
        }

        public static string GeneratePassword(int length)
        {
            if (length < 3)
            {
                throw new ArgumentException("Độ dài mật khẩu phải lớn hơn hoặc bằng 3 để đảm bảo các yêu cầu.");
            }

            // Danh sách các ký tự
            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()_-+=<>?";
            const string allChars = upperCase + lowerCase + digits;

            Random random = new Random();

            // Đảm bảo có ít nhất 1 ký tự viết hoa, 1 ký tự đặc biệt
            string upper = upperCase[random.Next(upperCase.Length)].ToString();
            string special = specialChars[random.Next(specialChars.Length)].ToString();
            string number = digits[random.Next(digits.Length)].ToString();

            // Các ký tự còn lại được chọn ngẫu nhiên
            string remainingChars = new string(Enumerable.Repeat(allChars, length - 3)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Ghép lại tất cả và xáo trộn vị trí
            string password = upper + special + number + remainingChars;
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }

        public static string GenerateLaterPartPassword(int length)
        {
            if (length < 3)
            {
                throw new ArgumentException("Độ dài mật khẩu phải lớn hơn hoặc bằng 3 để đảm bảo các yêu cầu.");
            }

            Random random = new Random();

            const string Letters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            const string Digits = "123456789";

            StringBuilder sb = new StringBuilder();
            sb.Append(Digits[random.Next(Digits.Length)]);

            for (int i = 1; i < length; i++)
            {
                string chars = Letters + Digits;
                sb.Append(chars[random.Next(chars.Length)]);
            }

            return new string(sb.ToString().OrderBy(_ => random.Next()).ToArray());
        }

        public static string JoinWithComma(ICollection<string> items)
        {
            // Kiểm tra nếu danh sách rỗng hoặc null
            if (items == null || items.Count == 0)
            {
                return string.Empty;
            }

            // Sử dụng string.Join để nối các phần tử với dấu phẩy
            return string.Join(", ", items);
        }

        public static bool IsValidPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return false;
            }

            phoneNumber = phoneNumber.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase);

            string pattern = @"^(0\d{9})$|^(84\d{9})$|^\+84\d{9}$|^[1-9]\d{8}$";

            Regex regex = new Regex(pattern);

            return regex.IsMatch(phoneNumber);
        }

        public static string NormalizeToDomesticFormat(string? phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return string.Empty;
            }

            var stringComparison = StringComparison.InvariantCultureIgnoreCase;
            phoneNumber = phoneNumber.Replace(" ", "", stringComparison);

            if (phoneNumber.StartsWith("+84", stringComparison) && phoneNumber.Length == 12)
            {
                return string.Concat("0", phoneNumber.AsSpan(3));
            }
            else if (phoneNumber.StartsWith("84", stringComparison) && phoneNumber.Length == 11)
            {
                return string.Concat("0", phoneNumber.AsSpan(2));
            }
            else if (phoneNumber.StartsWith("0", stringComparison) && phoneNumber.Length == 10)
            {
                return phoneNumber;
            }
            else if (!phoneNumber.StartsWith("0", stringComparison) && phoneNumber.Length == 9)
            {
                return "0" + phoneNumber;
            }

            return phoneNumber;
        }

        public static string FormatStringWithParam(object data, params object[]? param)
        {
            string objStr = data?.ToString() ?? string.Empty;
            return string.Format(objStr, param ?? Array.Empty<object>());
        }

        public static bool ContainsSpecialChars(string input)
        {
            return Regex.IsMatch(input, @"^[\p{L}\s]+$");
        }

        public static class TextCleaner
        {
            // Hàm chuẩn hóa chuỗi: trim, lowercase, chuẩn hóa khoảng trắng
            private static string NormalizeWhitespaceAndCase(string input)
            {
                return Regex.Replace(input.Trim().ToLowerInvariant(), @"\s+", " ");
            }

            // Hàm loại bỏ toàn bộ dấu câu
            public static string RemovePunctuation(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return string.Empty;
                }
                input = DecodeEscapesSmart(input);
                return Regex.Replace(input, @"[^\w\s]", "");
            }

            public static string DecodeEscapesSmart(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    return input;
                }
                string result = input;
                string pattern = @"\\[nrtbfv0\\'""]";
                bool changed = true;

                while (changed)
                {
                    string replaced = Regex.Replace(result, pattern, match =>
                    {
                        return match.Value switch
                        {
                            "\\n" => "\n",
                            "\\t" => "\t",
                            "\\r" => "\r",
                            "\\b" => "\b",
                            "\\f" => "\f",
                            "\\v" => "\v",
                            "\\0" => "\0",
                            "\\\\" => "\\",
                            "\\\"" => "\"",
                            "\\\'" => "'",
                            _ => match.Value
                        };
                    });

                    changed = replaced != result;
                    result = replaced;
                }
                return result;
            }

            // Hàm chuẩn hóa + loại bỏ dấu câu cho một chuỗi
            public static string CleanText(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return string.Empty;
                }
                string noPunctuation = RemovePunctuation(input);
                return NormalizeWhitespaceAndCase(noPunctuation);
            }

            // Hàm xử lý danh sách đáp án
            public static IList<string> CleanAnswers(IList<string>? answers)
            {
                if (answers == null || answers.Count == 0)
                {
                    return answers ?? new List<string>();
                }
                return answers.Select(ans => CleanText(ans)).ToList();
            }
        }

        public static (string? FirstName, string? LastName) ParseFullName(this string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (string.Empty, null);
            }

            int firstSpaceIndex = fullName.IndexOf(' ', StringComparison.InvariantCulture);

            if (firstSpaceIndex == -1)
            {
                return (fullName.Trim(), null);
            }

            string lastName = fullName[..firstSpaceIndex].Trim();
            string firstName = fullName[(firstSpaceIndex + 1)..].Trim();

            return (firstName, lastName);
        }

        public static string InjectParam(this string input, params string[] parameters)
        {
            if (string.IsNullOrEmpty(input) || parameters == null || parameters.Length == 0)
            {
                return input ?? string.Empty;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                string placeholder = $"{{{{{i}}}}}";
                input = input.Replace(placeholder, parameters[i] ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
            }
            return input;
        }

        public static T DecodeUrlBase64ToObject<T>(this string urlBase64)
        {
            if (string.IsNullOrWhiteSpace(urlBase64))
            {
                return default;
            }

            try
            {
                string decodedBase64 = HttpUtility.UrlDecode(urlBase64);
                byte[] data = Convert.FromBase64String(decodedBase64);
                string jsonString = Encoding.UTF8.GetString(data);

                // Deserialize thành object
                return JsonSerializer.Deserialize<T>(jsonString);
            }
            catch
            {
                return default;
            }
        }

        public static string EncodeObjectToUrlBase64(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            // Serialize object thành JSON
            string jsonString = JsonSerializer.Serialize(obj);

            // Chuyển sang base64
            byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
            string base64String = Convert.ToBase64String(bytes);

            // Encode URL để đảm bảo an toàn
            return HttpUtility.UrlEncode(base64String);
        }

        public static string ToSafeString(this string? input, string replace = "")
        {
            if (string.IsNullOrEmpty(input))
            {
                return replace;
            }
            return input.Trim();
        }
    }
}
