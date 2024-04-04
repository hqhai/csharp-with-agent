// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Globalization;
    using System.IO;
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

        public static string? CreateDigitalSignature(string jsonData, string privateKeyPath, HashAlgorithmName hash)
        {
            try
            {
                string privateKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, privateKeyPath);
                string privateKey = File.ReadAllText(privateKeyFile);

                using RSA rsa = RSA.Create();

                rsa.ImportFromPem(privateKey);

                string xmlData = rsa.ToXmlString(true);

                rsa.FromXmlString(xmlData);

                byte[] jsonDataBytes = Encoding.UTF8.GetBytes(jsonData);
                byte[] hashValue;

                hashValue = SHA256.HashData(jsonDataBytes);

                byte[] signature = rsa.SignHash(hashValue, hash, RSASignaturePadding.Pkcs1);
                string signatureBase64 = Convert.ToBase64String(signature);

                return signatureBase64;
            }
            catch
            {
                return null;
            }
        }

        public static string SecureHash(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = SHA512.HashData(bytes);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                stringBuilder.Append(b.ToString("x2", CultureInfo.CurrentCulture));
            }

            return stringBuilder.ToString();
        }

        public static string GenerateChecksum(string checksumKey, string data)
        {
            string checksumData = checksumKey + data;
            byte[] dataBytes = Encoding.UTF8.GetBytes(checksumData);
            byte[] hashValue = SHA512.HashData(dataBytes);
            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashValue)
            {
                builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
