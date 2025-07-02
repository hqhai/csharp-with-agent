// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class ExportReportStudentLearningProcessQueueModel
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public Guid? StudentId { get; set; }
        public EnumCourseType CourseType { get; set; }
        public string? FileName { get; set; }
        public string? UserNameStr { get; set; }
    }

    public class ExportReportSchoolLearningProcessQueueModel
    {
        public string? EventCodeStr { get; set; }
        public string? DistrictName { get; set; }
        public EnumCourseType CourseType { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public string? FileName { get; set; }
    }
}
