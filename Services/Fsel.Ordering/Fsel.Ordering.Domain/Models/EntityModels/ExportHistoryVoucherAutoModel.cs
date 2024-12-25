// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using OfficeOpenXml.Attributes;

    public class ExportHistoryVoucherAutoModel
    {
        [EpplusTableColumn(Header = "Voucher Code")]
        public string? VoucherCode { get; set; }

        [EpplusTableColumn(Header = "Status")]
        public string? UsageStatus { get; set; }

        [EpplusTableColumn(Header = "Order Code")]
        public string? OrderCode { get; set; }

        [EpplusTableColumn(Header = "Date Of Use")]
        public string? DayUsed { get; set; }

        [EpplusTableColumn(Header = "Email")]
        public string? Email { get; set; }
    }
}
