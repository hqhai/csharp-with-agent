// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Admins
{
    using OfficeOpenXml.Attributes;

    public class ImportAccountDashboardCommandModel
    {
        [EpplusTableColumn(Header = "Full Name")]
        public string? FullName { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "Username")]
        public string? UserName { get; set; }

        [EpplusTableColumn(Header = "EventCode")]
        public string? EventCode { get; set; }
    }
}
