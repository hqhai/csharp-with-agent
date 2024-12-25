// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.CompetitionEvent
{
    using OfficeOpenXml.Attributes;

    public class ExportLandingPageByEventCodeModel
    {
        [EpplusTableColumn(Header = "Họ và tên đệm")]
        public string? FirstName { get; set; }

        [EpplusTableColumn(Header = "Tên")]
        public string? LastName { get; set; }

        [EpplusTableColumn(Header = "Email đăng kí")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Ngày sinh")]
        public DateTime BirthDay { get; set; }

        [EpplusTableColumn(Header = "Email phụ huynh")]
        public string? ParentEmail { get; set; }

        [EpplusTableColumn(Header = "Số điện thoại phụ huynh")]
        public string? ParentPhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Vị trí trường học")]
        public string? Address { get; set; }

        [EpplusTableColumn(Header = "Trường học")]
        public string? School { get; set; }

        [EpplusTableColumn(Header = "Khối")]
        public string? SchoolGrade { get; set; }

        [EpplusTableColumn(Header = "Lớp")]
        public string? SchoolClass { get; set; }

        [EpplusTableColumn(Header = "Ischeckbox")]
        public bool IsBussinessCheckBox { get; set; }

        [EpplusTableColumn(Header = "Mã số sinh viên")]
        public string? SchoolStudentCode { get; set; }

        [EpplusTableColumn(Header = "Chuyên ngành")]
        public string? StudentMainMajor { get; set; }

        [EpplusTableColumn(Header = "Thời gian đăng kí")]
        public DateTime CreatedDate { get; set; }
    }
}
