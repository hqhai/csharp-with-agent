// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class StudentProgressExportModel
    {
        [EpplusTableColumn(Header = "Họ và tên")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Gmail")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Course Name")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "Trạng Thái")]
        public string? Status { get; set; }

        [EpplusTableColumn(Header = "Tiến độ học tập theo %")]
        public double? ProgressPercent { get; set; }

        [EpplusTableColumn(Header = "Vị trí học hiện tại (Lesson_Unit)")]
        public string? LocationName { get; set; }

        [EpplusTableColumn(Header = "Tổng thời gian truy cập")]
        public double TotalTime { get; set; }

        [EpplusTableColumn(Header = "Thời gian bắt đầu học", NumberFormat = "dd/MM/yyyy H:mm:ss")]
        public DateTime? ProgressDate { get; set; }

        [EpplusTableColumn(Header = "Ngày hết hạn", NumberFormat = "dd/MM/yyyy H:mm:ss")]
        public DateTime? ExpiredDate { get; set; }
    }
}
