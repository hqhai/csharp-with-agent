using System.Security.Cryptography;

namespace Fsel.Identity.Common.Helpers
{
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