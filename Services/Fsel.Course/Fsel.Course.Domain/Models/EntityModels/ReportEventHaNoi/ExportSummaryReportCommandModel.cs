// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ReportEventHaNoi
{
    using Microsoft.EntityFrameworkCore;

    [Keyless]
    public class ExportSummaryReportCommandModel
    {
        public string? DistrictName { get; set; }
        public string? SchoolName { get; set; }
        public int? ElevationOfTerrain { get; set; }
        public int? ElevationOfRefHeight { get; set; }
        public int? TotalStudentDefault { get; set; }
        public int? RegisterStudent { get; set; }
        public int? RegisterTeacher { get; set; }
        public int? TotalStudent { get; set; }
        public int? Level { get; set; }
        public int? ElevationOfTerrainTHCS { get; set; }
        public int? ElevationOfRefHeightTHCS { get; set; }
        public int? TotalStudentDefaultTHCS { get; set; }
        public int? RegisterStudentTHCS { get; set; }
        public int? RegisterTeacherTHCS { get; set; }
        public int? TotalStudentTHCS { get; set; }
        public int? TotalSchool { get; set; }
        public int? TotalSchoolTHCS { get; set; }
        public int? TotalSchoolDefault { get; set; }
        public int? TotalSchoolTHCSDefault { get; set; }
    }
}


