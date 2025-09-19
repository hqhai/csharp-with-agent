// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Campus
{
    using OfficeOpenXml.Attributes;

    public class AddStudentsToSchoolClassModel
    {
        [EpplusTableColumn(Header = "Họ và tên")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Ngày sinh")]
        public DateTime DateOfBirth { get; set; }
    }
}
