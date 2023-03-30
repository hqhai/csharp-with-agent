using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fsel.Common.Helpers
{
    public static class ConvertHelper
    {
        public static Dictionary<string, dynamic?> ObjectToDictionaryDynamic(object obj)
        {
            return obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(obj, null));
        }

        public static string ByteArrayToBase64(byte[] content)
        {
            return Convert.ToBase64String(content);
        }

        public static byte[] Base64ToByteArray(string base64Content)
        {
            return Convert.FromBase64String(base64Content);
        }

        public static Stream Base64ToStream(string base64Content)
        {
            var content = Base64ToByteArray(base64Content);
            Stream stream = new MemoryStream(content);
            return stream;
        }

        public static string StreamToString(Stream stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static string ObjectToBase64(object data)
        {
            var json = JsonSerializer.Serialize(data);
            var plainTextBytes = System.Text.Encoding.ASCII.GetBytes(json);
            return ByteArrayToBase64(plainTextBytes);
        }

        public static Stream ByteArrayToStream(byte[] input)
        {
            return new MemoryStream(input);
        }

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("empty string");
            }
            char[] arr = str.ToCharArray();
            arr[0] = char.ToUpper(arr[0]);
            return new string(arr);
        }

        public static string Serialize(this object? data, bool isCamelCase = false)
        {
            var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
            if (isCamelCase)
            {
                options.PropertyNameCaseInsensitive = true;
            }
            string jsonString = JsonSerializer.Serialize(data, options);
            return jsonString;
        }

        public static T? Deserialize<T>(this string? data, bool isCamelCase = false)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    return default;
                }

                var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
                if (isCamelCase)
                {
                    options.PropertyNameCaseInsensitive = true;
                }
                T? obj = JsonSerializer.Deserialize<T>(data, options);
                return obj;
            }
            catch
            {
                return default;
            }
        }

        public static T? Deserialize<T>(this object? objects)
        {
            try
            {
                var data = objects.Serialize();
                return Deserialize<T>(data);
            }
            catch
            {
                return default;
            }
        }

        public static T? DeserializeFromFilePath<T>(string path, bool isCamelCase = false)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string jsonString = sr.ReadToEnd();
                return Deserialize<T>(jsonString, isCamelCase);
            }
        }

        public static TEnum? EnumParse<TEnum>(this string? data) where TEnum : Enum
        {
            if (Enum.TryParse(typeof(TEnum), data, false, out object? result))
            {
                return (TEnum)result;
            }
            return default;
        }

        public static IList<TEnum> EnumToList<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
        }

        public static IList<string> EnumToListStr<TEnum>() where TEnum : Enum
        {
            return EnumToList<TEnum>().Select(x => x.ToString()).ToList();
        }
    }
}
