// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeSections
{
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.CommandModels.ExamPracticeAISettings;
    using Fsel.ExamPractice.Domain.Models.CommandModels.Questions;
    using Fsel.Shared.Enums;

    public class UpdateExamPracticeSectionCommandModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public EnumSectionExamPracticeType? Type { get; set; }
        public SectionMediaConfig? Config { get; set; }
        public IList<UpdateExamPracticeSectionCommandModel> ChildrenExamPracticeSections { get; set; } = new List<UpdateExamPracticeSectionCommandModel>();
        public IList<UpdateQuestionCommandModel> Questions { get; set; } = new List<UpdateQuestionCommandModel>();
        public IList<ExamPracticeAISettingCommandModel> ExamPracticeAISettings { get; set; } = new List<ExamPracticeAISettingCommandModel>();
    }
}
