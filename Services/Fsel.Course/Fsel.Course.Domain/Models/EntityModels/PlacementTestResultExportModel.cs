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
        public EnumCourseLevel LevelDone { get; set; }

        [EpplusTableColumn(Header = "Level students choose")]
        public EnumCourseLevel Level { get; set; }
    }
}
