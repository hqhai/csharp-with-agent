// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class ExportHistoryRedeemProductModel
    {
        [EpplusTableColumn(Header = "Code")]
        public string? Code { get; set; }

        [EpplusTableColumn(Header = "Product Code")]
        public string? ProductCode { get; set; }

        [EpplusTableColumn(Header = "Full Name")]
        public string? StudentName { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }

        [EpplusTableColumn(Header = "School")]
        public string? School { get; set; }

        [EpplusTableColumn(Header = "Requested")]
        public string? CreatedDate { get; set; }

        [EpplusTableColumn(Header = "Received")]
        public string? UpdatedDate { get; set; }
    }
}
