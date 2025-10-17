// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Students
{
    using OfficeOpenXml.Attributes;

    public class CreateStudentAndParentToEventFromFileModel
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

        [EpplusTableColumn(Header = "Parent Full Name\n(Họ và tên phụ huynh)")]
        public string? ParentFullName { get; set; }

        [EpplusTableColumn(Header = "Parent Phone Number\n(Số điện thoại phụ huynh)")]
        public string? ParentPhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Parent Email\n(Hòm thư điện tử phụ huynh)")]
        public string? ParentEmail { get; set; }

        [EpplusTableColumn(Header = "Parent Date Of Birth\n(Ngày sinh phụ huynh)")]
        public DateTime? ParentDateOfBirth { get; set; }
    }
}
