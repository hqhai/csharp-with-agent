// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Campus
{
    using OfficeOpenXml.Attributes;

    public class UpdateInfoStudentCampusModel
    {
        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Mã sinh viên")]
        public string? StudentCode { get; set; }

        [EpplusTableColumn(Header = "Lớp")]
        public string? SchoolClass { get; set; }
    }
}
