// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Entities.SkillScoreConfigs;

    public class ExamPracticeReportViewModel
    {
        public string? Name { get; set; }
        public Guid ExamPracticeResultId { get; set; }
        public IList<ExamPracticeReportSkillViewModel> Sections { get; set; } = new List<ExamPracticeReportSkillViewModel>();
    }

    public class ExamPracticeReportSkillViewModel
    {
        public Guid ExamPracticeSectionId { get; set; }
        public IList<SkillScores>? SkillScores { get; set; }
        public IList<ExamPracticeReportSkillViewModel>? Childrens { get; set; }
    }
}
