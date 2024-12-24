// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Shared.Enums;

    public class CourseLevelProgressModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public int TotalStudent { get; set; }
        public IList<OverallTestResultModel>? OverallTestResults { get; set; }
    }

    public class OverallTestResultModel
    {
        public string? Type { get; set; }
        public int Index { get; set; }
        public double Score { get; set; }
        public int TotalStudent { get; set; }
    }
}
