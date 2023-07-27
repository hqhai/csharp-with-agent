// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class UserVoucher : Entity
    {
        public EnumUserVoucherStatus Status { get; set; }

        public Guid UserId { get; set; }

        public Guid VoucherId { get; set; }

        public Voucher? Voucher { get; set; }
    }
}
