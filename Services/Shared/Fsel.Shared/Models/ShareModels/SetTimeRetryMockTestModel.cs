// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SetTimeRetryMockTestModel : ChatGptConfigModel
    {
        public Guid SectionId { get; set; }
        public Guid SectionGroupId { get; set; }
        public string? WordContent { get; set; }
        public Guid MockTestResultId { get; set; }
        public bool IsRetry { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class SetTimeRetryTestModel : ChatGptConfigModel
    {
        public Guid SectionId { get; set; }
        public Guid SectionGroupId { get; set; }
        public string? WordContent { get; set; }
        public Guid TestResultId { get; set; }
        public bool IsRetry { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class ChatGptConfigModel
    {
        public string? SettingModel { get; set; }

        public double SettingTemperature { get; set; }

        public double SettingWordMaxLength { get; set; }

        public double SettingTopP { get; set; }

        public double SettingFrequecy { get; set; }

        public double SettingPresence { get; set; }

        public string? SystemRoleAlConfig { get; set; }

        public string? UserAIConfig { get; set; }
    }

    public class SetTimeRetryClassForumModel : ChatGptConfigModel
    {
        public Guid ClassForumResultId { get; set; }

        public Guid ClassForumDetailResultId { get; set; }

        public string? WordContent { get; set; }
        public bool? IsRetry { get; set; } = false;

        public int DisplayOrder { get; set; }

        public EnumSubmissionCount SubmissionCount { get; set; }

        public DateTime StartDate { get; set; }
    }
}
