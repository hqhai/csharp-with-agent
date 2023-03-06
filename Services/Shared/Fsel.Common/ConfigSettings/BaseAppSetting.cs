namespace Fsel.Common.ConfigSettings
{
    public class BaseAppSetting
    {
        public Jwt? Jwt { get; set; }
    }

    public class Jwt
    {
        public string? SecretKey { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? Subject { get; set; }
        public int TokenValidityInMinutes { get; set; }
        public int RefreshTokenValidityInDays { get; set; }
    }
}