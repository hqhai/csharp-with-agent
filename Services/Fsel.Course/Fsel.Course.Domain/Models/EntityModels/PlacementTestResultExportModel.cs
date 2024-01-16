// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class PlacementTestResultExportModel
    {
        [EpplusTableColumn(Header = "FullName")]
        public string? Name { get; set; }

        [EpplusTableColumn(Header = "The student's student level achieved")]
        public EnumCourseLevel? LevelCompleted { get; set; }

        [EpplusTableColumn(Header = "Level students choose")]
        public EnumCourseLevel CurrentLevel { get; set; }

        [EpplusTableColumn(Header = "Finish day", NumberFormat = "dd/MM/yyyy")]
        public DateTime? UpdatedDate { get; set; }
    }
}
