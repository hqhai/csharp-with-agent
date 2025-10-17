// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;

namespace Fsel.System.Infrastructure.ValueSettings
{
    public class AppSetting : BaseAppSetting
    {
        public Smtp? Smtp { get; set; }
        public Otp? Otp { get; set; }
        public ConstantUrl? ConstantUrl { get; set; }
        public GoogleSheetConfig? GoogleSheetConfig { get; set; }
        public SharePointConfig? SharePointConfig { get; set; }
        public DailyQuizConfig? DailyQuizConfig { get; set; }
        public new Services? Services { get; set; }

        public OpenAiConfig? OpenAiConfig { get; set; }
        public ConnectionStrings? ConnectionStrings { get; set; }
    }

    public class ConnectionStrings
    {
        public string? CrmConnection { get; set; }
    }

    public class OpenAiConfig
    {
        public string? Uri { get; set; }
        public string? ApiKey { get; set; }
    }

    public class SharePointConfig
    {
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? TenantId { get; set; }
        public string? FSELContentTeamSiteId { get; set; }
        public string? FSELContentTeamFileId { get; set; }
        public string? FSELContentTeamSheetName { get; set; }
    }

    public class ConstantUrl
    {
        public string? ConfirmOtpUrl { get; set; }
    }

    public class GoogleSheetConfig
    {
        public string? I18NSpreadSheetId { get; set; }
        public string? I18NSheetVN { get; set; }
        public string? I18NSheetEN { get; set; }
        public string? I18NSheetFR { get; set; }
        public string? MailMarketingSpreadSheetId { get; set; }
        public string? MailMarketingSheet { get; set; }
        public string? LandingPageSpreadSheetId { get; set; }
        public string? LandingPageSheet { get; set; }
        public string? LandingPageFSELSpreadSheetId { get; set; }
        public string? LandingPageFSELSheet { get; set; }
        public string? CCEmailSpreadSheetId { get; set; }
        public string? CCEmailSheet { get; set; }
        public string? OrderInfoSpreadSheetId { get; set; }
        public string? OrderInfoSheet { get; set; }
        public string? SchoolStudentSheetId { get; set; }
        public string? LuckyTicketSpreadSheetId { get; set; }
        public string? VoucherForMASpreadSheetId { get; set; }
        public string? RegisterStudentForEventSpreadSheetId { get; set; }
        public string? ErrorReportExplanationQuestionId { get; set; }
        public string? ErrorReportExplanationQuestion { get; set; }
    }

    public class Services : BaseServices
    {
        public string? LmsCourseApiUrl { get; set; }
        public string? OrderApiUrl { get; set; }
        public string? DictionaryApiUrl { get; set; }
        public string? StorageApiUrl { get; set; }
        public string? FFmpegApiUrl { get; set; }
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

    public class DailyQuizConfig
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberQuestion { get; set; }
        public int NumberCorrect { get; set; }
        public int NumberWinner { get; set; }
        public int EndHour { get; set; }
    }
}
