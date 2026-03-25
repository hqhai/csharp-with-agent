// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels.StudentDashboard
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Response model cho màn hình Tỉ lệ hoàn thành khóa học
    /// </summary>
    public class CourseCompletionResponseModel
    {
        /// <summary>
        /// KPI - Số học sinh hoàn thành khóa học
        /// </summary>
        public int TotalCompletedStudents { get; set; }

        /// <summary>
        /// KPI - Tổng số học viên đã vào học
        /// </summary>
        public int TotalEnteredStudents { get; set; }

        /// <summary>
        /// KPI - Tỉ lệ hoàn thành (%)
        /// </summary>
        public double CompletionRate { get; set; }

        /// <summary>
        /// Bảng Top 10 học sinh hoàn thành khóa học
        /// </summary>
        public IList<CourseCompletionTopStudentModel> TopStudents { get; set; } = new List<CourseCompletionTopStudentModel>();
    }

    /// <summary>
    /// Model cho bảng Top 10 học sinh hoàn thành khóa học
    /// </summary>
    public class CourseCompletionTopStudentModel
    {
        /// <summary>
        /// Id học sinh
        /// </summary>
        public Guid StudentId { get; set; }

        /// <summary>
        /// Họ tên học sinh
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Tên trường
        /// </summary>
        public string? SchoolName { get; set; }

        /// <summary>
        /// Tên tỉnh/thành
        /// </summary>
        public string? ProvinceName { get; set; }

        /// <summary>
        /// Tên khóa học
        /// </summary>
        public string? CourseName { get; set; }

        /// <summary>
        /// Số bài đã hoàn thành
        /// </summary>
        public int CompletedLessons { get; set; }

        /// <summary>
        /// Số bài mục tiêu
        /// </summary>
        public int TargetLessons { get; set; }

        /// <summary>
        /// Ngày hoàn thành khóa học
        /// </summary>
        public DateTime? CompletedDate { get; set; }

        /// <summary>
        /// Tên trình độ
        /// </summary>
        public string? LevelName { get; set; }

        /// <summary>
        /// Phần trăm hoàn thành khóa học
        /// </summary>
        public decimal? Percent { get; set; }
    }
}
