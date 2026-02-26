// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections
{
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Questions;
    using Fsel.ExamPractice.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;

    public class CreateExamPracticeSectionCommandModel
    {
        public string? Name { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public EnumSectionExamPracticeType? Type { get; set; }
        public Guid? AiPromptManagerId { get; set; }
        public SectionMediaConfig? Config { get; set; }
        public IList<CreateExamPracticeSectionCommandModel> ChildrenExamPracticeSections { get; set; } = new List<CreateExamPracticeSectionCommandModel>();
        public IList<CreateQuestionCommandModel> Questions { get; set; } = new List<CreateQuestionCommandModel>();
        public IList<ExamPracticeAISettingModel> ExamPracticeAISettings { get; set; } = new List<ExamPracticeAISettingModel>();
    }
}
