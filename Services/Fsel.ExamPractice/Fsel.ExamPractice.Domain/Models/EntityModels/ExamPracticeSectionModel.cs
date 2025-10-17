// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.Shared.Enums;

    public class ExamPracticeSectionModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public SectionMediaConfig? Config { get; set; }
        public EnumSectionExamPracticeType? Type { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? ParentExamPracticeSectionId { get; set; }
        public IList<ExamPracticeSectionModel> ExamPracticeSections { get; set; } = new List<ExamPracticeSectionModel>();
        public IList<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
        public IList<ExamPracticeAISettingModel> ExamPracticeAISettings { get; set; } = new List<ExamPracticeAISettingModel>();
    }
}
