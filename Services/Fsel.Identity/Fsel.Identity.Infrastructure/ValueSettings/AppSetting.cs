// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;

namespace Fsel.Identity.Infrastructure.ValueSettings
{
    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
        public Otp? Otp { get; set; }
        public ConstantUrl? ConstantUrl { get; set; }
        public new Services? Services { get; set; }
        public Authentication? Authentication { get; set; }
    }

    public class ConstantUrl
    {
        public string? ConfirmOtpUrl { get; set; }
    }

    public class Services : BaseServices
    {
        public string? InteractionApiUrl { get; set; }
        public string? TrainingApiUrl { get; set; }
        public string? OrderApiUrl { get; set; }
        public string? LmsCourseApiUrl { get; set; }
        public string? SystemApiUrl { get; set; }
    }

    public class Authentication
    {
        public Google? Google { get; set; }
        public Facebook? Facebook { get; set; }
    }

    public class Google
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public IList<string>? RedirectUriParams { get; set; }
    }

    public class Facebook
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? Callback { get; set; }
    }

    public class Otp
    {
        public int StepTime { get; set; }
        public int StepDayWithAdmin { get; set; }
    }

    public class Smtp
    {
        public string? From { get; set; }
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
