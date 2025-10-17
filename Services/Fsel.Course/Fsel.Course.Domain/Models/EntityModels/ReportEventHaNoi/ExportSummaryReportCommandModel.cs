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

        /// <summary>
        /// Số học sinh thực tế
        /// </summary>
        public int? ElevationOfTerrainTHCS { get; set; }

        /// <summary>
        /// Số GV Thực tế
        /// </summary>
        public int? ElevationOfRefHeightTHCS { get; set; }

        /// <summary>
        /// Tổng số học viên thực tế (HS + GV)
        /// </summary>
        public int? TotalStudentDefaultTHCS { get; set; }

        /// <summary>
        /// Số lượng học sinh đăng ký
        /// </summary>
        public int? RegisterStudentTHCS { get; set; }

        /// <summary>
        /// Số lượng GV đăng ký
        /// </summary>
        public int? RegisterTeacherTHCS { get; set; }

        /// <summary>
        /// Tổng số học viên đăng ký (HS+GV)
        /// </summary>
        public int? TotalStudentTHCS { get; set; }

        public int? TotalSchool { get; set; }
        public int? TotalSchoolTHCS { get; set; }
        public int? TotalSchoolDefault { get; set; }
        public int? TotalSchoolTHCSDefault { get; set; }

        // new
        public int? TotalStudentVerified { get; set; }

        public int? TotalStudentVerifiedTHCS { get; set; }
        public int? TotalTeacherVerified { get; set; }
        public int? TotalTeacherVerifiedTHCS { get; set; }
        public int? TotalStudentPTComplete { get; set; }
        public int? TotalStudentPTProgress { get; set; }
        public int? TotalStudentPTCompleteTHCS { get; set; }
        public int? TotalStudentPTProgressTHCS { get; set; }
        public int? TotalTeacherPTComplete { get; set; }
        public int? TotalTeacherPTProgress { get; set; }
        public int? TotalTeacherPTCompleteTHCS { get; set; }
        public int? TotalTeacherPTProgressTHCS { get; set; }
        public int? TotalStudentLearnProgressTHCS { get; set; }
        public int? TotalStudentLearnProgress { get; set; }
        public int? TotalTeacherLearnProgress { get; set; }
        public int? TotalTeacherLearnProgressTHCS { get; set; }
    }
}
