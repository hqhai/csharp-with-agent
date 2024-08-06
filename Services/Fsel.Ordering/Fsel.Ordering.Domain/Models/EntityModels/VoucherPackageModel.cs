// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class VoucherPackageModel : BaseModel
    {
        public Guid PackageId { get; set; }
        public Guid VoucherId { get; set; }
    }
}
