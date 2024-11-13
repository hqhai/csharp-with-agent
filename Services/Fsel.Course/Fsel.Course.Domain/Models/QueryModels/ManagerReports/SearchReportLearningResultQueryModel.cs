// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ManagerReports
{
    using Fsel.Shared.Enums;

    public class SearchReportLearningResultQueryModel : BaseSearchStudentReportQueryModel
    {
        public EnumOverallScore? OverallScore { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumLearningStatus? LearningStatus { get; set; }
    }
}
