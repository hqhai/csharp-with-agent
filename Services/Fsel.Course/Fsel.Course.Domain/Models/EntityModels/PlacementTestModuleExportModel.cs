// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class PlacementTestModuleExportModel
    {
        [EpplusTableColumn(Header = "FullName")]
        public string? Name { get; set; }

        [EpplusTableColumn(Header = "Birthday", NumberFormat = "dd/MM/yyyy")]
        public DateTime? Birthday { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Level")]
        public EnumPlacementTestLevel Level { get; set; }

        [EpplusTableColumn(Header = "CorrectCount")]
        public double CorrectCount { get; set; }

        [EpplusTableColumn(Header = "CorrectTotal")]
        public double CorrectTotal { get; set; }

        [EpplusTableColumn(Header = "SkillScores")]
        public string? SkillScoresStr { get; set; }
    }
}
