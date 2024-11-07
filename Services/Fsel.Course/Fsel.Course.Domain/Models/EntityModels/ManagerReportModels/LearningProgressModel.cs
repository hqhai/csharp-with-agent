// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class LearningProgressModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? SchoolName { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? ContentProgress { get; set; }
        public string? UnitName { get; set; }
        public string? LessonName { get; set; }

        public string? StatusDescription
        {
            get
            {
                return Status.GetDescription();
            }
        }

        public EnumLearningStatus Status { get; set; }
    }
}
