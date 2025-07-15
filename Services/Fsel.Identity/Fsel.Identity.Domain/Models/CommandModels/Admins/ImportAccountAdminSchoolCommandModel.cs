// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using OfficeOpenXml.Attributes;

    public class ImportAccountAdminSchoolCommandModel
    {
        [EpplusTableColumn(Header = "Tên trường")]
        public string? SchoolName { get; set; }

        [EpplusTableColumn(Header = "LocalId")]
        public string? LocalId { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Username")]
        public string? UserName { get; set; }

        [EpplusTableColumn(Header = "Mật khẩu")]
        public string? Password { get; set; }
    }
}
