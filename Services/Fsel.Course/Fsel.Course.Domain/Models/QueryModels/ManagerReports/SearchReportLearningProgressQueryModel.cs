// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;

    public class SearchReportLearningProgressQueryModel : BaseSearchStudentReportQuery
    {
        public string? SchoolClass { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
    }
}
