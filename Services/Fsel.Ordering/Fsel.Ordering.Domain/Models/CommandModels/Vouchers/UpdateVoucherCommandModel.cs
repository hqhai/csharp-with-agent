// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class UpdateVoucherCommandModel
    {
        public Guid Id { get; set; }
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
