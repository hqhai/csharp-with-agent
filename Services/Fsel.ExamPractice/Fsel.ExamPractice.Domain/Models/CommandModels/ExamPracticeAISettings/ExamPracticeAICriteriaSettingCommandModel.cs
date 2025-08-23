// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAISettings
{
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;
    using Fsel.Shared.Enums;

    public class ExamPracticeAICriteriaSettingCommandModel
    {
        public Guid? Id { get; set; }
        public string? SystemRoleAlConfig { get; set; }
        public IList<ExamPracticePromptModel>? Prompts { get; set; }
        public EnumMockTestAIType CriteriaName { get; set; }
    }
}
