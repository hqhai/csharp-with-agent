// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Helpers
{
    using System;
    using System.Security.Cryptography;

    public static class TokenHelper
    {
        public static string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);

                return Convert.ToBase64String(random);
            }
        }
    }
}
