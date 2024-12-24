// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;

    public class CheckVoucherModel
    {
        public Guid? VoucherId { get; set; }
        public EnumVoucherCategory? Category { get; set; }
        public int? Value { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public UserVoucherLock? UserVoucherLock { get; set; }
    }
}
