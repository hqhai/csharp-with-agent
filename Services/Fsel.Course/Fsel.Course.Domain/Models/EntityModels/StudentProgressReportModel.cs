// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class StudentProgressReportModel
    {
        [EpplusTableColumn(Header = "Họ và tên")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Trường")]
        public string? School { get; set; }

        [EpplusTableColumn(Header = "Gmail")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Course Name")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "Trạng Thái")]
        public string? StatusUser { get; set; }

        [EpplusTableColumn(Header = "Tiến độ học tập theo %")]
        public double ProgressPercent { get; set; }

        [EpplusTableColumn(Header = "Vị trí học hiện tại (Lesson_Unit)")]
        public string? CurrentPosition { get; set; }

        [EpplusTableColumn(Header = "Unit 1 overall")]
        public double OverallUnit1 { get; set; }

        [EpplusTableColumn(Header = "Unit 2 overall")]
        public double OverallUnit2 { get; set; }

        [EpplusTableColumn(Header = "Unit 3 overall")]
        public double OverallUnit3 { get; set; }

        [EpplusTableColumn(Header = "Unit 4 overall")]
        public double OverallUnit4 { get; set; }

        [EpplusTableColumn(Header = "Unit 5 overall")]
        public double OverallUnit5 { get; set; }

        [EpplusTableColumn(Header = "Unit 6 overall")]
        public double OverallUnit6 { get; set; }

        [EpplusTableColumn(Header = "Unit 7 overall")]
        public double OverallUnit7 { get; set; }

        [EpplusTableColumn(Header = "Unit 8 overall")]
        public double OverallUnit8 { get; set; }

        [EpplusTableColumn(Header = "Unit 9 overall")]
        public double OverallUnit9 { get; set; }

        [EpplusTableColumn(Header = "Unit 10 overall")]
        public double OverallUnit10 { get; set; }

        [EpplusTableColumn(Header = "Unit 11 overall")]
        public double OverallUnit11 { get; set; }

        [EpplusTableColumn(Header = "Unit 12 overall")]
        public double OverallUnit12 { get; set; }

        [EpplusTableColumn(Header = "Overall theo leaderboard")]
        public double LeaderboardPercent { get; set; }

        [EpplusTableColumn(Header = "Learn Time")]
        public double LearnTime { get; set; }

        [EpplusTableColumn(Header = "Social Time")]
        public double SocialTime { get; set; }

        [EpplusTableColumn(Header = "Other Time")]
        public double OtherTime { get; set; }

        [EpplusTableColumn(Header = "Tổng số lần truy cập")]
        public double TotalVisit { get; set; }

        [EpplusTableColumn(Header = "Thời gian bắt đầu học", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? ProcessDate { get; set; }

        [EpplusTableColumn(Header = "Ngày hết hạn", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? ExpiredDate { get; set; }
    }
}
