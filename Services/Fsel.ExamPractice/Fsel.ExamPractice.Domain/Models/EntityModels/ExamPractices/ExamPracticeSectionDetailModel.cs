// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Entities.Configs;
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.ExamPractice.Domain.Models.EntityModels.Questions;
    using Fsel.Shared.Enums;

    public class ExamPracticeSectionDetailModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public SectionMediaConfig? Config { get; set; }
        public Guid? ParentExamPracticeSectionId { get; set; }
        public EnumSectionExamPracticeType? Type { get; set; }
        public double ExecutionTime { get; set; }
        public IList<ExamPracticeSectionDetailModel> ExamPracticeSections { get; set; } = new List<ExamPracticeSectionDetailModel>();
        public ExamPracticeSectionResultModel? ExamPracticeSectionResult { get; set; }
        public IList<Guid> QuestionIds { get; set; } = new List<Guid>();
        public IList<QuestionCorrectStatusModel> QuestionTests { get; set; } = new List<QuestionCorrectStatusModel>();
        public object? Answer { get; set; }
    }
}
