// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Ordering.Domain.Entities;

    public class CheckVoucherModel
    {
        public Guid VoucherId { get; set; }
        public int Percent { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public UserVoucherLock? UserVoucherLock { get; set; }
    }
}
