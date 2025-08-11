// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Enums;

    public class ExamPracticeDetailModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeSubType SubType { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolYear { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? ExecutionTime { get; set; }
        public ExamPracticeResultModel? ExamPracticeResult { get; set; }
        public IList<ExamPracticeSectionDetailModel> ExamPracticeSections { get; set; } = new List<ExamPracticeSectionDetailModel>();
    }
}
