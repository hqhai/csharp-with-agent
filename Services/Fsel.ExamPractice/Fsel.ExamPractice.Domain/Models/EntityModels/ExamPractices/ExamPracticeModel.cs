// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.ExamPractice.Domain.Enums;

    public class ExamPracticeModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeSubType SubType { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolYear { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Province { get; set; }
        public Guid? ProvinceId { get; set; }
        public int? ExecutionTime { get; set; }
        public EnumExamPracticeStatus Status { get; set; }
        public int TotalAttempts { get; set; }
        public Guid OriginalId { get; set; }
        public int Version { get; set; }
        public EnumVersionStatus VersionStatus { get; set; }
        public IList<ExamPracticeSectionModel> ExamPracticeSections { get; set; } = new List<ExamPracticeSectionModel>();
        public ExamPracticeResultModel? ExamPracticeResult { get; set; }
    }
}
