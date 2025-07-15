// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.ValueSettings
{
    using Fsel.Common.ValueSettings;

    public class AppSetting : BaseAppSetting
    {
        public ConstantUrl? ConstantUrl { get; set; }
        public ResourceContent? ResourceContent { get; set; }
        public OpenAiConfig? OpenAiConfig { get; set; }
        public new Services? Services { get; set; }
        public AzureAiConfig? AzureAiConfig { get; set; }

        public CustomerSupportConfig? CustomerSupportConfig { get; set; }

        public GoogleSheetConfig? GoogleSheetConfig { get; set; }

        public CacheConfig? CacheConfig { get; set; }

        public TouchpointConfig? TouchpointConfig { get; set; }
    }

    public class ConstantUrl
    {
        public string? LinkFullMockTestReport { get; set; }
        public string? LinkMockTestReport { get; set; }
    }

    public class CustomerSupportConfig
    {
        public string? Email { get; set; }
        public IList<string> CCEmail { get; set; } = new List<string>();
    }

    public class GoogleSheetConfig
    {
        public string? StudentGetErrorClassForumSheetId { get; set; }
    }

    public class ResourceContent
    {
        public string? LmsWebsiteUrl { get; set; }
        public string? HotLine { get; set; }
    }

    public class OpenAiConfig
    {
        public string? Uri { get; set; }
        public IList<string>? ApiKeys { get; set; }

        public string? ApprovalAIModel { get; set; }
    }

    public class AzureAiConfig
    {
        public string? FirstApiKey { get; set; }
        public string? SecondApiKey { get; set; }
        public string? Location { get; set; }
    }

    public class Services : BaseServices
    {
        public string? InteractionApiUrl { get; set; }
        public string? TrainingApiUrl { get; set; }
        public string? SystemApiUrl { get; set; }
        public string? OrderApiUrl { get; set; }
        public string? NotificationApiUrl { get; set; }
        public string? StorageApiUrl { get; set; }
        public string? FFmpegApiUrl { get; set; }
    }

    public class CacheConfig
    {
        public bool TurnOnCaching { get; set; }
        public int CachingDuration { get; set; }
    }

    public class TouchpointConfig
    {
        public bool TurnOnTouchpoint { get; set; }
    }
}
