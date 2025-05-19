// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    using Fsel.ExamPractice.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExamPracticeSearchModel
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedFullName { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public EnumExamPracticeType Type { get; set; }
        public EnumExamPracticeSubType SubType { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IList<EnumCourseSkill> CourseSkills { get; set; } = new List<EnumCourseSkill>();
        public EnumExamPracticeSubType CourseSubType { get; set; }

        public string? SchoolYear
        {
            get
            {
                return StartDate.HasValue && EndDate.HasValue ? StartDate.Value.Year + " - " + EndDate.Value.Year : null;
            }
        }

        public int TotalAttempts { get; set; }
        public EnumExamPracticeStatus Status { get; set; }
    }
}
