// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using System;
    using Fsel.Shared.Enums;

    public class CreateVoucherCommandModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Percent { get; set; }
        public int Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EnumVoucherType VoucherType { get; set; }
        public IList<Guid>? PackageIds { get; set; }
    }
}
