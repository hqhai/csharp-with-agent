// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Sender.Domain.ValueSettings
{
    using Fsel.Common.ValueSettings;
    using Fsel.Shared.Enums;

    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
        public SmtpGoogle? SmtpGoogle { get; set; }
        public EmailConfig? EmailConfig { get; set; }
        public SMSConfig? SMSConfig { get; set; }
        public ZaloConfig? ZaloConfig { get; set; }
        public new Services? Services { get; set; }
    }

    public class SMSConfig
    {
        public EnumSMSBrand UseBrand { get; set; }
        public IRISConfig? IRISConfig { get; set; }
        public GAPITConfig? GAPITConfig { get; set; }
    }

    public class IRISConfig
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? BrandName { get; set; }
        public string? ContentType { get; set; }
        public string? GrantType { get; set; }
    }

    public class GAPITConfig
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? BrandName { get; set; }
        public string? ContentType { get; set; }
        public string? ServiceId { get; set; }
        public string? CPId { get; set; }
    }

    public class EmailConfig
    {
        public ICollection<string>? BCCEmail { get; set; }
    }

    public class Smtp
    {
        public string? From { get; set; }
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? AwsAccessKeyId { get; set; }
        public string? AwsSecretAccessKey { get; set; }
    }

    public class ZaloConfig
    {
        public string? BrandName { get; set; }
        public string? Token { get; set; }
        public string? OTPTemplateId { get; set; }
    }

    public class SmtpGoogle
    {
        public string? From { get; set; }
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class Services : BaseServices
    {
        public string? SystemApiUrl { get; set; }
        public string? IRISApiUrlDC { get; set; }
        public string? IRISApiUrlDR { get; set; }
        public string? GAPITApiUrl { get; set; }
    }
}
