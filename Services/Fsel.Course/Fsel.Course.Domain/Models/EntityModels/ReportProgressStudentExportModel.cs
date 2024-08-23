// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class ReportProgressStudentExportModel
    {
        [EpplusTableColumn(Header = "FullName")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Birthday", NumberFormat = "dd/MM/yyyy")]
        public DateTime? Birthday { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Course Name")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "Unit Name")]
        public string? UnitName { get; set; }

        [EpplusTableColumn(Header = "Unit Status")]
        public EnumResultStatus? UnitStatus { get; set; }

        [EpplusTableColumn(Header = "Lesson Name")]
        public string? LessonName { get; set; }

        [EpplusTableColumn(Header = "Lesson Status")]
        public EnumResultStatus? LessonStatus { get; set; }

        [EpplusTableColumn(Header = "Video Status")]
        public EnumResultStatus? VideoStatus { get; set; }

        [EpplusTableColumn(Header = "ClassForum Status")]
        public EnumClassForumResultStatus? ClassForumStatus { get; set; }

        [EpplusTableColumn(Header = "HomeWork Status")]
        public EnumResultStatus? HomeWorkStatus { get; set; }

        [EpplusTableColumn(Header = "Final study period", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? FinalStudyPeriod { get; set; }

        [EpplusTableColumn(Header = "Last entry", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? LastEntry { get; set; }
    }
}
