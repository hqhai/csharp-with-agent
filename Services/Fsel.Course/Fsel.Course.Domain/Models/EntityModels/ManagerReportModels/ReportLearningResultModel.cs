// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class ReportLearningResultModel
    {
        public Guid StudentId { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public double Percent { get; set; }
        public int DisplayOrder { get; set; }
        public int TotalStudent { get; set; }
        public string? SchoolClass { get; set; }
    }
}
