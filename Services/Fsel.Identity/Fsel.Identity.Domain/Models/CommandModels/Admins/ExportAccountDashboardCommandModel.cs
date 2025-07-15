// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using OfficeOpenXml.Attributes;

    public class ExportAccountDashboardCommandModel
    {
        [EpplusTableColumn(Header = "Full Name")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Role")]
        public string? Role { get; set; }

        [EpplusTableColumn(Header = "EventCode")]
        public string? EventCode { get; set; }

        [EpplusTableColumn(Header = "Ngày tạo")]
        public DateTime? CreatedDate { get; set; }

        [EpplusTableColumn(Header = "Username")]
        public string? UserName { get; set; }

        [EpplusTableColumn(Header = "Password")]
        public string? DefaultPassword { get; set; }

    }
}
