// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Security.Cryptography;
    using System.Text;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Models.ShareModels;

    public static class TokenConfigHelper
    {
        public static T? GetTokenConfig<T>(this TokenConfigModel? tokenConfigModel) where T : class
        {
            if (tokenConfigModel == null)
            {
                return default;
            }

            return tokenConfigModel.Config?.Deserialize<T>();
        }

        public static string GetMd5Hash(this string input)
        {
            using var md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder();
            foreach (byte b in data)
            {
                sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
