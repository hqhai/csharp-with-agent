// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels
{
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;
    using Fsel.Shared.Enums;

    public class ExamPracticeAICriteriaSettingModel
    {
        public Guid Id { get; set; }
        public string? SystemRoleAlConfig { get; set; }
        public IList<ExamPracticePromptModel>? Prompts { get; set; }
        public EnumMockTestAIType CriteriaName { get; set; }
        public Guid? ExamPracticeAISettingId { get; set; }
    }
}
