// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Shared.Enums;

    public class OverallReportLearningProgressModel
    {
        public long TotalStudent { get; set; }
        public IList<CourseLevelProgressModel>? CourseLevelProgresses { get; set; }
        public string? ContentAverageProgress { get; set; }
        public IList<CourseTypeStudentModel>? CourseTypeStudents { get; set; }
    }

    public class CourseTypeStudentModel
    {
        public EnumCourseType CourseType { get; set; }
        public long TotalStudent { get; set; }
    }
}
