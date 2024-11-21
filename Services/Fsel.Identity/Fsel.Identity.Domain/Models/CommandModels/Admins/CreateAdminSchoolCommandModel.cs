// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using OfficeOpenXml.Attributes;

    public class CreateAdminSchoolCommandModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Họ và tên")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Password")]
        public string? Password { get; set; }

        [EpplusTableColumn(Header = "School Id")]
        public string? SchoolId { get; set; }
    }
}
