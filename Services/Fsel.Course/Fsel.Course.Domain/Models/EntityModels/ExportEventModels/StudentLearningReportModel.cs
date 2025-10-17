// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ExportEventModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class StudentLearningReportModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? SchoolClass { get; set; }

        public bool NotLoggedIn { get; set; }             // Chưa đăng nhập
        public bool LoggedInButNoPT { get; set; }         // Đăng nhập mà chưa PT
        public bool PTButNotStudied { get; set; }         // PT mà chưa học
        public bool SelectedLessonButNotStudied { get; set; } // Chọn bài mà chưa học

        public EnumCourseLevel? CurrentLevel { get; set; }         // Trình độ hiện tại
        public EnumCourseLevel? SuggetLevel { get; set; }          // Trình độ Sugget PT
        public string? CourseName { get; set; }
        public int DaysSinceLastAccess { get; set; }

        public DateTime? PaymentDate { get; set; }        // Ngày thanh toán
        public DateTime? FirstStudyDate { get; set; }     // Ngày học đầu tiên
        public DateTime? ExpiredDate { get; set; }
        public string? ProgressModule { get; set; }       // Học phần
        public double? ProgressPercent { get; set; }      // Phần trăm tiến độ
        public IList<WeeklyProgressModel> WeeklyResults { get; set; } = new List<WeeklyProgressModel>();
    }

    public class WeeklyProgressModel
    {
        public int WeekNumber { get; set; }
        public DateTime WeekStartDate { get; set; }
        public DateTime WeekEndDate { get; set; }
        public string? LearnedLesson { get; set; }
        public string? PresentLesson { get; set; }
        public string? TargetLesson { get; set; }
        public int RequiredLessons { get; set; }
        public int CompletedLessons { get; set; }
        public string? Status { get; set; }
    }
}
