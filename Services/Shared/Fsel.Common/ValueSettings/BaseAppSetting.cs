// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.ValueSettings
{
    public class BaseAppSetting
    {
        public Jwt? Jwt { get; set; }
        public string? ServiceName { get; set; }
        public Services? Services { get; set; }
    }

    public class Services
    {
        public string? SenderApiUrl { get; set; }
        public string? UserApiUrl { get; set; }
        public string? ClassApiUrl { get; set; }
        public string? StorageApiUrl { get; set; }
        public string? SystemApiUrl { get; set; }
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
