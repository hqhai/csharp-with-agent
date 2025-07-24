// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;

namespace Fsel.Identity.Infrastructure.ValueSettings
{
    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
        public Otp? Otp { get; set; }
        public ConstantUrl? ConstantUrl { get; set; }
        public ResourceContent? ResourceContent { get; set; }
        public new Services? Services { get; set; }
        public GoogleSheetConfig? GoogleSheetConfig { get; set; }
        public UserReferralConfig? UserReferralConfig { get; set; }
        public UserDeletionConfig? UserDeletionConfig { get; set; }
        public CacheConfig? CacheConfig { get; set; }
        public CRMConfig? CRMConfig { get; set; }
    }

    public class UserReferralConfig
    {
        public int PT { get; set; }
        public int DoneUnit1 { get; set; }
    }

    public class ResourceContent
    {
        public string? LmsWebsiteUrl { get; set; }
        public string? HotLine { get; set; }
    }

    public class ConstantUrl
    {
        public string? ConfirmOtpUrl { get; set; }
        public string? RegisterUrl { get; set; }
        public string? LinkResetProgress { get; set; }
    }

    public class Services : BaseServices
    {
        public string? InteractionApiUrl { get; set; }
        public string? TrainingApiUrl { get; set; }
        public string? OrderApiUrl { get; set; }
        public string? LmsCourseApiUrl { get; set; }
        public string? SystemApiUrl { get; set; }
        public string? ClassApiUrl { get; set; }
    }

    public class Otp
    {
        public int StepTime { get; set; }
        public int StepDayWithAdmin { get; set; }
        public bool IsByPassOtp { get; set; }
        public string? ByPassOtpValue { get; set; }
        public int MaxSendOtpSms { get; set; }
    }

    public class Smtp
    {
        public string? From { get; set; }
        public string? SmtpServer { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class GoogleSheetConfig
    {
        public string? SchoolStudentSheetId { get; set; }
        public string? CreateUsersAndOrders { get; set; }
        public string? AddCoinSpreadSheetId { get; set; }
        public string? AddCoinBuyCourseSpreadSheetId { get; set; }
        public string? AddCoinFselEventSpreadSheetId { get; set; }
        public string? SummerSelfLearningSpreadSheetId { get; set; }
        public string? SummerSelfLearningSheet { get; set; }
        public string? StudentRegisterFormSheetId { get; set; }
        public string? StudentRegisterFormSheet { get; set; }
    }

    public class UserDeletionConfig
    {
        public int DeletionDays { get; set; }
        public int DeletionMinutes { get; set; }
    }

    public class CacheConfig
    {
        public bool TurnOnCaching { get; set; }
        public int CachingDuration { get; set; }
    }

    public class CRMConfig
    {
        public string? SecretKey { get; set; }
    }
}
