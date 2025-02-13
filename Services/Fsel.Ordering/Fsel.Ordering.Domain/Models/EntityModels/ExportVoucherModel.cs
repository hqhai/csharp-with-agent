// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class ExportVoucherModel
    {
        [EpplusTableColumn(Header = "Voucher code")]
        public string? Code { get; set; }

        [EpplusTableColumn(Header = "Voucher name")]
        public string? Name { get; set; }

        [EpplusTableColumn(Header = "Creation date")]
        public string? CreatedDate { get; set; }

        [EpplusTableColumn(Header = "Quantity")]
        public int Quantity { get; set; }

        [EpplusTableColumn(Header = "Used")]
        public int QuantityUsed { get; set; }

        [EpplusTableColumn(Header = "Remaining")]
        public int RemainingQuantity { get; set; }

        [EpplusTableColumn(Header = "Status")]
        public string? ItemStatus { get; set; }

        [EpplusTableColumn(Header = "Discount form")]
        public EnumVoucherCategory Category { get; set; }

        [EpplusTableColumn(Header = "Discount")]
        public int Value { get; set; }

        [EpplusTableColumn(Header = "Duration")]
        public string? Duration { get; set; }
    }
}
