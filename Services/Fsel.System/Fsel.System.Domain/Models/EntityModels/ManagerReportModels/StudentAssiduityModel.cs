// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class StudentAssiduityModel
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? SchoolName { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }

        public string? CourseLevelStr
        {
            get
            {
                return CourseLevel.HasValue ? CourseLevel.Value.GetDescription() : null;
            }
        }

        public long TotalTimeVideo { get; set; }

        public string? TotalTimeVideoStr
        {
            get { return SendMailHelper.FormatTimeSpanAsClock(Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(TotalTimeVideo)); }
        }

        public long TotalTimeClassForum { get; set; }

        public string? TotalTimeClassForumStr
        {
            get { return SendMailHelper.FormatTimeSpanAsClock(Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(TotalTimeClassForum)); }
        }

        public long TotalTimeHomeWork { get; set; }

        public string? TotalTimeHomeWorkStr
        {
            get { return SendMailHelper.FormatTimeSpanAsClock(Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(TotalTimeHomeWork)); }
        }

        public long TotalTime { get; set; }

        public string? TotalTimeStr
        {
            get { return SendMailHelper.FormatTimeSpanAsClock(Shared.Helpers.DateTimeHelper.ConvertSecondsToMinutes(TotalTime)); }
        }

        public long TotalVisit { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CurrentDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
    }
}
