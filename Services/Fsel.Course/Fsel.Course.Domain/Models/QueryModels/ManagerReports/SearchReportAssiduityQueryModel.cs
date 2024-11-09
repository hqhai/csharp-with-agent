// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;

    public class SearchReportAssiduityQueryModel : BaseSearchStudentReportQuery
    {
        public EnumCourseType? CourseType { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
