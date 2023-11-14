// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
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
    }
}
