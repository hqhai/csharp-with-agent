// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.SenderTemplates
{
    using Fsel.Shared.Enums;

    public class WeeklyReportModel
    {
        public EnumSenderTemplate SenderTemplate { get; set; }
        public string? FullName { get; set; }
        public string? SkillScores { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? TotalDay { get; set; }
        #region không sửa, không xóa
        public string? SaturdayIsActive { get; set; }
        public string? SundayIsActive { get; set; }
        public string? MondayIsActive { get; set; }
        public string? TuesdayIsActive { get; set; }
        public string? WednesdayIsActive { get; set; }
        public string? ThursdayIsActive { get; set; }
        public string? FridayIsActive { get; set; }
        #endregion
        public string? TotalHour { get; set; }
        public string? TotalLearn { get; set; }
        public string? TotalSocial { get; set; }
        public string? TotalOther { get; set; }
        public string? CoursePercent { get; set; }
        public string? ContinueLearn { get; set; }
        public string? TotalDailyStreak { get; set; }
        public string? PreviousTotal { get; set; }
        public string? PreviousLearn { get; set; }
        public string? PreviousSocial { get; set; }
        public string? PreviousOther { get; set; }
        public string? DailyStreak { get; set; }
        public string? NoDailyStreak { get; set; }
        public string? NextUnit { get; set; }
        public int? NextLesson { get; set; }
        public int? PercentLesson { get; set; }
        public string? Weekly3Display { get; set; }
        public string? ColorTotal { get; set; }
        public string? ColorLearn { get; set; }
        public string? ColorSocial { get; set; }
        public string? ColorOther { get; set; }
        public string? IsLessonDone { get; set; }
        public string? SkillMockTestDisplay { get; set; }
        public string? SkillMockTest { get; set; }
        public string? AccessLink { get; set; }
    }
}
