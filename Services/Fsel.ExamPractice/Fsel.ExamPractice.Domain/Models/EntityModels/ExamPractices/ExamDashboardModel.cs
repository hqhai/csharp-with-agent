// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices
{
    public class ExamDashboardModel
    {
        /// <summary>
        /// Tổng số đề mà user đã hoàn thành.
        /// </summary>
        public double CompletedCount { get; set; }

        /// <summary>
        /// Tổng số đề hiện có trên hệ thống.
        /// </summary>
        public double TotalCount { get; set; }

        /// <summary>
        /// Điểm trung bình các bài thi (0..10).
        /// </summary>
        public double AverageScore { get; set; }

        /// <summary>
        /// Bậc (VSTEP Level) tương ứng với điểm trung bình.
        /// </summary>
        public string? AverageLevel { get; set; }

        /// <summary>
        /// Điểm overall (có thể = điểm gần nhất hoặc tính riêng).
        /// </summary>
        public double OverallScore { get; set; }

        /// <summary>
        /// Bậc tương ứng với overall score.
        /// </summary>
        public string? OverallLevel { get; set; }
    }
}
