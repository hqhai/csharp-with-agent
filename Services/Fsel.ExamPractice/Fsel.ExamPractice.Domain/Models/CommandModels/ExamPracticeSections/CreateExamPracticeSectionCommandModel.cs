// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections
{
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.AiGradeSettings;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class CreateExamPracticeSectionCommandModel
    {
        public string? Name { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public EnumSectionExamPracticeType? Type { get; set; }
        public SectionMediaConfig? SectionMediaConfig { get; set; }
        public IList<CreateExamPracticeSectionCommandModel> ChildrenExamPracticeSections { get; set; } = new List<CreateExamPracticeSectionCommandModel>();
        public IList<CreateQuestionCommandModel> Questions { get; set; } = new List<CreateQuestionCommandModel>();
        public IList<ExamPracticeAISettingModel> ExamPracticeAISettings { get; set; } = new List<ExamPracticeAISettingModel>();
    }

    public class ExamPracticeAISettingModel
    {
        public bool IsUseAIGrade { get; set; }
        public string? SystemRoleAlConfig { get; set; }
        public string? UserAlConfig { get; set; }
        public string? SettingModel { get; set; }
        public double? SettingTemperature { get; set; }
        public double? SettingWordMaxLength { get; set; }
        public double? SettingTopP { get; set; }
        public double? SettingFrequecy { get; set; }
        public double? SettingPresence { get; set; }
        public string? Task { get; set; }
        public IList<ExamPracticePromptModel>? Prompts { get; set; }
        public IList<ExamPracticeAICriteriaSettingModel>? ExamPracticeAICriteriaSettings { get; set; }
    }

    public class ExamPracticeAICriteriaSettingModel
    {
        public string? SystemRoleAlConfig { get; set; }
        public string? PromptStr { get; set; }
        public IList<ExamPracticePromptModel>? Prompts { get; set; }
        public Guid? ExamPracticeAISettingId { get; set; }
        public EnumMockTestAIType CriteriaName { get; set; }
    }
}
