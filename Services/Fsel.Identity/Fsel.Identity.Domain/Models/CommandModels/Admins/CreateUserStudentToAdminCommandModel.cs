// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using OfficeOpenXml.Attributes;

    public class CreateUserStudentToAdminCommandModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Họ và tên")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Năm sinh")]
        public string? DateOfBirth { get; set; }

        [EpplusTableColumn(Header = "Giới tính")]
        public string? Gender { get; set; }

        [EpplusTableColumn(Header = "Trường ")]
        public string? School { get; set; }

        [EpplusTableColumn(Header = "Trường Id")]
        public Guid? SchoolId { get; set; }
    }
}
