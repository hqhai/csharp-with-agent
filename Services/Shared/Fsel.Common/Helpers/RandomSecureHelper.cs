// Copyright (c) Atlantic. All rights reserved.

using System.Security.Cryptography;

namespace Fsel.Common.Helpers
{
    public class RandomSecureHelper
    {
        private readonly RNGCryptoServiceProvider _rngProvider = new RNGCryptoServiceProvider();

        public int Next()
        {
            var randomBuffer = new byte[4];
            _rngProvider.GetBytes(randomBuffer);
            var result = BitConverter.ToInt32(randomBuffer, 0);
            return result;
        }

        public int Next(int maximumValue)
        {
            return Next(0, maximumValue);
        }

        public int Next(int minimumValue, int maximumValue)
        {
            var seed = Next();
            return new Random(seed).Next(minimumValue, maximumValue);
        }

        public string Secretstrings()
        {
            var randomBytes = new byte[32];
            _rngProvider.GetBytes(randomBytes);
            string secret = BitConverter.ToString(randomBytes).Replace("-", "");
            return secret;
        }
    }
}
