// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class LearningResultModel
    {
        public Guid StudentId { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? SchoolName { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }

        public string? CourseLevelStr
        {
            get
            {
                return CourseLevel.HasValue ? CourseLevel.Value.GetDescription() : null;
            }
        }

        public double OverallPercent { get; set; }
        public IList<OverallModuleReportModel> OverallModules { get; set; } = new List<OverallModuleReportModel>();
        public DateTime? ProcessDate { get; set; }
        public EnumLearningStatus Status { get; set; }

        public string? StatusStr
        {
            get
            {
                return Status.GetDescription();
            }
        }

        public DateTime? ExpiredDate { get; set; }
    }
}
