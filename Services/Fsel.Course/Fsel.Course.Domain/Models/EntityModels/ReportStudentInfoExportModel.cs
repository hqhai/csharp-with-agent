// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class ReportStudentInfoExportModel
    {
        [EpplusTableColumn(Header = "FullName")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Birthday", NumberFormat = "dd/MM/yyyy")]
        public DateTime? Birthday { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "PhoneNumber")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "CreatedDate", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? CreatedDate { get; set; }

        [EpplusTableColumn(Header = "School")]
        public string? SchoolName { get; set; }

        [EpplusTableColumn(Header = "District")]
        public string? DistrictName { get; set; }

        [EpplusTableColumn(Header = "Province/City")]
        public string? Province { get; set; }

        [EpplusTableColumn(Header = "PlacmentTest Status")]
        public EnumResultStatus? PTStatus { get; set; }

        [EpplusTableColumn(Header = "PT Level")]
        public EnumCourseLevel? PTLevel { get; set; }

        [EpplusTableColumn(Header = "Course Name")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "Course Level")]
        public string? CourseLevel { get; set; }

        [EpplusTableColumn(Header = "Start Trial", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? StartTrial { get; set; }

        [EpplusTableColumn(Header = "End Trial", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? EndTrial { get; set; }

        [EpplusTableColumn(Header = "Unit")]
        public string? UnitName { get; set; }

        [EpplusTableColumn(Header = "Unit Average")]
        public double? UnitAverage { get; set; }

        [EpplusTableColumn(Header = "Lesson")]
        public string? LessonName { get; set; }

        [EpplusTableColumn(Header = "Lesson Completed")]
        public string? LessonCompleted { get; set; }

        [EpplusTableColumn(Header = "Total time")]
        public long? TotalTime { get; set; }

        [EpplusTableColumn(Header = "Total access")]
        public int? TotalAccess { get; set; }

        [EpplusTableColumn(Header = "Final study period", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? FinalStudyPeriod { get; set; }

        [EpplusTableColumn(Header = "Last entry", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? LastEntry { get; set; }
    }
}
