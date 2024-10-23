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

        [EpplusTableColumn(Header = "Ngày sinh")]
        public string? DateOfBirth { get; set; }

        [EpplusTableColumn(Header = "Giới tính")]
        public string? Gender { get; set; }

        [EpplusTableColumn(Header = "Level")]
        public string? CourseLevel { get; set; }

        [EpplusTableColumn(Header = "CourseId")]
        public string? CourseId { get; set; }

        [EpplusTableColumn(Header = "Trường")]
        public string? School { get; set; }

        [EpplusTableColumn(Header = "Trường Id")]
        public string? SchoolId { get; set; }

        [EpplusTableColumn(Header = "Gửi email")]
        public string? IsSendMail { get; set; }

        [EpplusTableColumn(Header = "Password")]
        public string? Password { get; set; }
    }
}
