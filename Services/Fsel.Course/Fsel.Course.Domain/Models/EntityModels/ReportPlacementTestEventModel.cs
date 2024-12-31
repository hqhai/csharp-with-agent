// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ReportPlacementTestEventModel
    {
        public string? LocationName { get; set; }

        /// <summary>
        /// Số trường đăng ký tham gia
        /// </summary>
        public int NumberRegisteredSchool { get; set; }

        /// <summary>
        /// Số trường tham gia thực tế
        /// </summary>
        public int NumberActualParticipatingSchool { get; set; }

        /// <summary>
        /// Tỉ lệ trường tham gia thực tế
        /// </summary>
        public double ActualSchoolParticipationRate
        {
            get
            {
                return NumberRegisteredSchool == 0 ? 0 : NumberHelper.GetPercent(NumberActualParticipatingSchool, NumberRegisteredSchool);
            }
        }

        /// <summary>
        /// Số học sinh đăng ký tài khoản hợp lệ
        /// </summary>
        public int NumberValidStudentAccount { get; set; }

        /// <summary>
        /// Số học sinh đã hoàn thành PT
        /// </summary>
        public int NumberStudentsCompletedPT { get; set; }

        /// <summary>
        /// Tỷ lệ HS Hoàn thành PT/Đăng ký TK
        /// </summary>
        public double CompletionRate
        {
            get
            {
                return NumberValidStudentAccount == 0 ? 0 : NumberHelper.GetPercent(NumberStudentsCompletedPT, NumberValidStudentAccount);
            }
        }

        public IList<ReportCourseLevelModel>? ReportCourseLevels { get; set; }
    }

    public class ReportCourseLevelModel
    {
        public EnumCourseLevel CourseLevel { get; set; }
        public long TotalStudent { get; set; }
        public double Percent { get; set; }
    }
}
