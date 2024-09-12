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

        [EpplusTableColumn(Header = "Tiến độ học tập theo %")]
        public double ProgressPercent { get; set; }

        [EpplusTableColumn(Header = "Vị trí học hiện tại (Lesson_Unit)")]
        public string? CurrentPosition { get; set; }

        [EpplusTableColumn(Header = "Unit 1 overall")]
        public double OverallUnitOne { get; set; }

        [EpplusTableColumn(Header = "Unit 2 overall")]
        public double OverallUnitTwo { get; set; }

        [EpplusTableColumn(Header = "Unit 3 overall")]
        public double OverallUnitThree { get; set; }

        [EpplusTableColumn(Header = "Unit 4 overall")]
        public double OverallUnitFour { get; set; }

        [EpplusTableColumn(Header = "Unit 5 overall")]
        public double OverallUnitFive { get; set; }

        [EpplusTableColumn(Header = "Unit 6 overall")]
        public double OverallUnitSix { get; set; }

        [EpplusTableColumn(Header = "Unit 7 overall")]
        public double OverallUnitSeven { get; set; }

        [EpplusTableColumn(Header = "Unit 8 overall")]
        public double OverallUnitEight { get; set; }

        [EpplusTableColumn(Header = "Unit 9 overall")]
        public double OverallUnitNine { get; set; }

        [EpplusTableColumn(Header = "Unit 10 overall")]
        public double OverallUnitTen { get; set; }

        [EpplusTableColumn(Header = "Unit 11 overall")]
        public double OverallUnitEleven { get; set; }

        [EpplusTableColumn(Header = "Unit 12 overall")]
        public double OverallUnitTwelve { get; set; }

        [EpplusTableColumn(Header = "Overall theo leaderboard")]
        public double LeaderboardPercent { get; set; }

        [EpplusTableColumn(Header = "Learn Time")]
        public double LearnTime { get; set; }

        [EpplusTableColumn(Header = "Social Time")]
        public double SocialTime { get; set; }

        [EpplusTableColumn(Header = "Tổng số lần truy cập")]
        public double TotalVisits { get; set; }

        [EpplusTableColumn(Header = "Tổng số lần truy cập", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? ProcessDate { get; set; }
    }
}
