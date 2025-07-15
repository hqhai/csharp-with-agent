// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using OfficeOpenXml.Attributes;

    public class CreateStudentToEventFromFileModel
    {
        [EpplusTableColumn(Header = "Full Name\n(Họ và tên)")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Phone Number\n(Số điện thoại)")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Email\n(Hòm thư điện tử)")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Date Of Birth\n(Ngày sinh)")]
        public DateTime? DateOfBirth { get; set; }

        [EpplusTableColumn(Header = "Grade\n(Khối học)")]
        public string? SchoolGrade { get; set; }

        [EpplusTableColumn(Header = "Class\n(Lớp)")]
        public string? SchoolClass { get; set; }
    }
}
