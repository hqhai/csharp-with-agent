using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                return string.Empty;

            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static string ObjectToBase64(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var plainTextBytes = System.Text.Encoding.ASCII.GetBytes(json);
            return ByteArrayToBase64(plainTextBytes);
        }

        public static Stream ByteArrayToStream(byte[] input)
        {
            return new MemoryStream(input);
        }

        public static void Capitalize(this JArray jArr)
        {
            foreach (var x in jArr.ToList())
            {
                var childObj = x as JObject;
                if (childObj != null)
                {
                    childObj.Capitalize();
                    continue;
                }
                var childArr = x as JArray;
                if (childArr != null)
                {
                    childArr.Capitalize();
                }
            }
        }

        public static void Capitalize(this JObject jObj)
        {
            foreach (var kvp in jObj.Cast<KeyValuePair<string, JToken>>().ToList())
            {
                jObj.Remove(kvp.Key);
                var newKey = kvp.Key.Capitalize();
                var childObj = kvp.Value as JObject;
                if (childObj != null)
                {
                    childObj.Capitalize();
                    jObj.Add(newKey, childObj);
                    return;
                }
                var childArr = kvp.Value as JArray;
                if (childArr != null)
                {
                    childArr.Capitalize();
                    jObj.Add(newKey, childArr);
                    return;
                }
                jObj.Add(newKey, kvp.Value);
            }
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

        public static string Serialize(this object? data)
        {
            string jsonString = JsonConvert.SerializeObject(data);
            return jsonString;
        }

        public static T? Deserialize<T>(this string? data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                {
                    return default;
                }
                T? obj = JsonConvert.DeserializeObject<T>(data);
                return obj;
            }
            catch
            {
                return default;
            }
        }
    }
}
