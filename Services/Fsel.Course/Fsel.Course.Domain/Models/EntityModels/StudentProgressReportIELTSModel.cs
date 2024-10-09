// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class StudentProgressReportIELTSModel
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

        [EpplusTableColumn(Header = "Nội dung hoàn thành")]
        public string? CompletedProgress { get; set; }

        [EpplusTableColumn(Header = "Tiến độ học tập theo %")]
        public double ProgressPercent { get; set; }

        [EpplusTableColumn(Header = "Vị trí học hiện tại Unit")]
        public string? CurrentPositionUnit { get; set; }

        [EpplusTableColumn(Header = "Vị trí học hiện tại Lesson")]
        public string? CurrentPositionLesson { get; set; }

        [EpplusTableColumn(Header = "Unit 1 overall")]
        public double? OverallUnit1 { get; set; }

        [EpplusTableColumn(Header = "Unit 2 overall")]
        public double? OverallUnit2 { get; set; }

        [EpplusTableColumn(Header = "Unit 3 overall")]
        public double? OverallUnit3 { get; set; }

        [EpplusTableColumn(Header = "Unit 4 overall")]
        public double? OverallUnit4 { get; set; }

        [EpplusTableColumn(Header = "Unit 5 overall")]
        public double? OverallUnit5 { get; set; }

        [EpplusTableColumn(Header = "Unit 6 overall")]
        public double? OverallUnit6 { get; set; }

        [EpplusTableColumn(Header = "Unit 7 overall")]
        public double? OverallUnit7 { get; set; }

        [EpplusTableColumn(Header = "Unit 8 overall")]
        public double? OverallUnit8 { get; set; }

        [EpplusTableColumn(Header = "Course Overall")]
        public double? CourseOverall { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 1")]
        public string? BandSkillMockTest1 { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 2")]
        public string? BandSkillMockTest2 { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 3")]
        public string? BandSkillMockTest3 { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 4")]
        public string? BandSkillMockTest4 { get; set; }

        [EpplusTableColumn(Header = "Full Mock test 1")]
        public string? BandFullMockTest1 { get; set; }

        [EpplusTableColumn(Header = "Reading - Full Mock test 1")]
        public string? BandFullMockTestReading1 { get; set; }

        [EpplusTableColumn(Header = "Listening - Full Mock test 1")]
        public string? BandFullMockTestListening1 { get; set; }

        [EpplusTableColumn(Header = "Writing - Full Mock test 1")]
        public string? BandFullMockTestWriting1 { get; set; }

        [EpplusTableColumn(Header = "Speaking - Full Mock test 1")]
        public string? BandFullMockTestSpeaking1 { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 5")]
        public string? BandSkillMockTest5 { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 6")]
        public string? BandSkillMockTest6 { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 7")]
        public string? BandSkillMockTest7 { get; set; }

        [EpplusTableColumn(Header = "Single-skill Mock test 8")]
        public string? BandSkillMockTest8 { get; set; }

        [EpplusTableColumn(Header = "Full Mock test 2")]
        public string? BandFullMockTest2 { get; set; }

        [EpplusTableColumn(Header = "Reading - Full Mock test 2")]
        public string? BandFullMockTestReading2 { get; set; }

        [EpplusTableColumn(Header = "Listening - Full Mock test 2")]
        public string? BandFullMockTestListening2 { get; set; }

        [EpplusTableColumn(Header = "Writing - Full Mock test 2")]
        public string? BandFullMockTestWriting2 { get; set; }

        [EpplusTableColumn(Header = "Speaking - Full Mock test 2")]
        public string? BandFullMockTestSpeaking2 { get; set; }

        [EpplusTableColumn(Header = "Thời gian truy cập - Video lesson")]
        public double TimeVideoLesson { get; set; }

        [EpplusTableColumn(Header = "Thời gian truy cập - Class forum")]
        public double TimeClassForum { get; set; }

        [EpplusTableColumn(Header = "Thời gian truy cập - Homework")]
        public double TimeHomeWork { get; set; }

        [EpplusTableColumn(Header = "Tổng thời gian truy cập")]
        public double TotalTime { get; set; }

        [EpplusTableColumn(Header = "Tổng số lần truy cập")]
        public double TotalVisit { get; set; }

        [EpplusTableColumn(Header = "Thời gian bắt đầu học", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? ProcessDate { get; set; }

        [EpplusTableColumn(Header = "Ngày hết hạn", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? ExpiredDate { get; set; }
    }
}
