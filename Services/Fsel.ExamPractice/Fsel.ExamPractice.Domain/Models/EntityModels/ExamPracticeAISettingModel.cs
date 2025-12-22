// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;

    public class ExamPracticeAISettingModel
    {
        public Guid Id { get; set; }
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
        public Guid? ExamPracticeSectionId { get; set; }
        public IList<ExamPracticeAICriteriaSettingModel>? ExamPracticeAICriteriaSettings { get; set; }
    }
}
