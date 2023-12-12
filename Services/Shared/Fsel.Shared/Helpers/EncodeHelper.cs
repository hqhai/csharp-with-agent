// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    public static class EncodeHelper
    {
        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2", CultureInfo.InvariantCulture));
                }
            }

            return hash.ToString();
        }
    }
}
