// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAISettings
{
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;

    public class ExamPracticeAISettingCommandModel
    {
        public Guid? Id { get; set; }
        public string? SystemRoleAlConfig { get; set; }
        public string? UserAlConfig { get; set; }
        public string? SettingModel { get; set; }
        public double SettingTemperature { get; set; }
        public double SettingWordMaxLength { get; set; }
        public double SettingTopP { get; set; }
        public double SettingFrequecy { get; set; }
        public double SettingPresence { get; set; }
        public string? Task { get; set; }
        public IList<ExamPracticePromptModel>? Prompts { get; set; }
        public IList<ExamPracticeAICriteriaSettingCommandModel>? ExamPracticeAICriteriaSettings { get; set; }
    }
}
