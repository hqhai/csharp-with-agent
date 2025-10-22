// Copyright (c) Atlantic. All rights reserved.

using System.Text.Json.Serialization;
using Fsel.Common.ValueSettings;

namespace Fsel.Ordering.Infrastructure.ValueSettings
{
    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
        public Otp? Otp { get; set; }
        public ConstantUrl? ConstantUrl { get; set; }
        public PaymentConfig? PaymentConfig { get; set; }
        public PayooConfig? PayooConfig { get; set; }
        public new Services? Services { get; set; }
        public UrBoxConfig? UrBoxConfig { get; set; }
        public VoucherConfigs? VoucherConfigs { get; set; }
        public PurchaseSettings? PurchaseSettings { get; set; }
        public ResourceContent? ResourceContent { get; set; }
        public BlindBoxConfigs? BlindBoxConfigs { get; set; }
        public OrderConfigs? OrderConfigs { get; set; }
        public MarketplacePremiumConfig? MarketplacePremiumConfig { get; set; }
    }

    public class BlindBoxConfigs
    {
        [JsonInclude]
        private int[]? _packages;

        public IReadOnlyList<int>? Packages
        {
            get => _packages;
            set => _packages = value?.ToArray();
        }
    }

    public class VoucherConfigs
    {
        public VoucherConfig? VoucherForRetail { get; set; }
    }

    public class VoucherConfig
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public int Quantity { get; set; }
        public int Percent { get; set; }
    }

    public class PurchaseSettings
    {
        public AppStore? AppStore { get; set; }
    }

    public class AppStore
    {
        public string? AppId { get; set; }
        public string? BundleId { get; set; }
        public string? KeyId { get; set; }
        public string? Audience { get; set; }
        public string? Issuer { get; set; }
    }

    public class ResourceContent
    {
        public string? LmsWebsiteUrl { get; set; }
        public string? HotLine { get; set; }
        public string? Email { get; set; }
    }

    public class ConstantUrl
    {
        public string? PaymentSuccessUrl { get; set; }
        public string? UpdateSenderSettingUrl { get; set; }
    }

    public class UrBoxConfig
    {
        public string? AppSecret { get; set; }
        public string? AppId { get; set; }
        public IList<string>? DisableCategories { get; set; }
    }

    public class PayooConfig
    {
        public string? Username { get; set; }
        public string? ShopId { get; set; }
        public string? ShopDomain { get; set; }
        public string? ShopBackUrl { get; set; }
        public string? NotifyUrl { get; set; }
        public string? Key { get; set; }
        public string? PayooIP { get; set; }
        public string? ShopTitle { get; set; }
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
        public string? UrBoxApiUrl { get; set; }
        public string? PayooApiUrl { get; set; }
        public string? AppStoreApiUrl { get; set; }
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

    public class OrderConfigs
    {
        public int TrialPeriod { get; set; }
    }

    public class MarketplacePremiumConfig
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? BrandImage { get; set; }
        public string? BrandName { get; set; }
        public DateTime? StartDateButton { get; set; }
        public DateTime? EndDateButton { get; set; }
    }
}
