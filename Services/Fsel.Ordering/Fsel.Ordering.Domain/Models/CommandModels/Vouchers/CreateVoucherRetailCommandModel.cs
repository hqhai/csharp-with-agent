// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.CommandModels.Vouchers
{
    using System;

    public class CreateVoucherRetailCommandModel
    {
        public Guid UserId { get; set; }
        public Guid PackageId { get; set; }
    }
}
