// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class PlacementTestResultExportModel
    {
        [EpplusTableColumn(Header = "FullName")]
        public string? Name { get; set; }

        [EpplusTableColumn(Header = "Birthday", NumberFormat = "dd/MM/yyyy")]
        public DateTime? Birthday { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Level students choose")]
        public EnumCourseLevel? CurrentLevel { get; set; }

        [EpplusTableColumn(Header = "Course")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "The student's student level achieved")]
        public EnumCourseLevel? LevelCompleted { get; set; }

        [EpplusTableColumn(Header = "LastModulePT")]
        public EnumCourseLevel? CourseLevel { get; set; }

        [EpplusTableColumn(Header = "Percentage of final PT")]
        public double Percent { get; set; }

        [EpplusTableColumn(Header = "Placement Test Completed")]
        public bool IsPTdone { get; set; }

        [EpplusTableColumn(Header = "Finish day", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? UpdatedDate { get; set; }
    }
}
