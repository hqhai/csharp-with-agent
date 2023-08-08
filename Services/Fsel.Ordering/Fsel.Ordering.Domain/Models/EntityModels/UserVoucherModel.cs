// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class UserVoucherModel : Entity
    {
        public EnumUserVoucherStatus Status { get; set; }

        public Guid UserId { get; set; }

        public Guid VoucherId { get; set; }

        public string? VoucherName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public VoucherModel? Voucher { get; set; }
    }
}
