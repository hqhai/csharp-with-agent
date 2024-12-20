// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class PlacementTestReportExportModel
    {
        [EpplusTableColumn(Header = "FullName")]
        public string? Name { get; set; }

        [EpplusTableColumn(Header = "Birthday", NumberFormat = "dd/MM/yyyy")]
        public DateTime? Birthday { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Current level")]
        public string? CurrentLevel { get; set; }

        [EpplusTableColumn(Header = "Current suggested level")]
        public string? SuggetLevel { get; set; }

        [EpplusTableColumn(Header = "Level students choose")]
        public string? ChooseLevel { get; set; }

        [EpplusTableColumn(Header = "Course")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "LastModulePT")]
        public string? CompletionLevel { get; set; }

        [EpplusTableColumn(Header = "Percentage of final PT")]
        public double? Percent { get; set; }

        [EpplusTableColumn(Header = "Placement Test Completed")]
        public bool IsPTdone { get; set; }

        [EpplusTableColumn(Header = "Finish day", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? FinishDate { get; set; }
    }
}
