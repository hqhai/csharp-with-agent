// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class PlacementTestResultExportModel
    {
        [EpplusTableColumn(Header = "Họ và tên")]
        public string? Name { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Ngày sinh", NumberFormat = "dd/MM/yyyy")]
        public DateTime? Birthday { get; set; }

        [EpplusTableColumn(Header = "Trường")]
        public string? SchoolName { get; set; }

        [EpplusTableColumn(Header = "Khối")]
        public string? SchoolGrade { get; set; }

        [EpplusTableColumn(Header = "Lớp")]
        public string? SchoolClass { get; set; }

        [EpplusTableColumn(Header = "Khóa học lựa chọn")]
        public EnumCourseLevel? CurrentLevel { get; set; }

        [EpplusTableColumn(Header = "Khóa học hiện tại")]
        public string? CourseName { get; set; }

        [EpplusTableColumn(Header = "Level được gợi ý")]
        public EnumCourseLevel? LevelCompleted { get; set; }

        [EpplusTableColumn(Header = "LastModulePT")]
        public EnumCourseLevel? CourseLevel { get; set; }

        [EpplusTableColumn(Header = "Percentage of final PT")]
        public double Percent { get; set; }

        [EpplusTableColumn(Header = "Trạng thái")]
        public bool IsPTdone { get; set; }

        [EpplusTableColumn(Header = "Ngày kết thúc", NumberFormat = "dd/MM/yyyy HH:mm:ss")]
        public DateTime? UpdatedDate { get; set; }
    }
}
