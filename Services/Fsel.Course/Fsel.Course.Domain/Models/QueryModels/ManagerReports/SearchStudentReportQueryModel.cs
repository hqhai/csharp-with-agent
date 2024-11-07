// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;

    public class SearchStudentReportQueryModel : BaseSearchStudentReportQuery
    {
        public DateTime? StartDate { get; set; }
        public EnumCompletionStatus? Status { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
        public string? SchoolClass { get; set; }
    }
}
