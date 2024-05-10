// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;
    using OfficeOpenXml.Attributes;

    public class ReportProgressStudentExportModel
    {
        [EpplusTableColumn(Header = "FullName")]
        public string? Name { get; set; }

        [EpplusTableColumn(Header = "Birthday", NumberFormat = "dd/MM/yyyy")]
        public DateTime? Birthday { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Course")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "Unit")]
        public string? UnitName { get; set; }

        [EpplusTableColumn(Header = "Lesson")]
        public string? LessonName { get; set; }

        [EpplusTableColumn(Header = "Status")]
        public EnumResultStatus Status { get; set; }

        [EpplusTableColumn(Header = "Last entry", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? UpdatedDate { get; set; }
    }
}
