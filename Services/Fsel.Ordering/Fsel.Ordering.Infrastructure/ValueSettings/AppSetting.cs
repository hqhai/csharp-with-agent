// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;

namespace Fsel.Ordering.Infrastructure.ValueSettings
{
    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
        public Otp? Otp { get; set; }
        public ConstantUrl? ConstantUrl { get; set; }
        public PaymentConfig? PaymentConfig { get; set; }
        public new Services? Services { get; set; }
    }

    public class ConstantUrl
    {
        public string? PaymentSuccessUrl { get; set; }
    }

    public class PaymentConfig
    {
        public VNPAY? VNPAY { get; set; }
    }

    public class VNPAY
    {
        public string? Version { get; set; }
        public string? VnpUrl { get; set; }
        public string? VnpTmnCode { get; set; }
        public string? VnpHashSecret { get; set; }
    }

    public class Services : BaseServices
    {
        public string? LmsCourseApiUrl { get; set; }
        public string? SystemApiUrl { get; set; }
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
