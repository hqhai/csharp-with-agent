// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Entities;
    using Fsel.Shared.Enums;

    public class VoucherModel : BaseModel
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public int Percent { get; set; }

        public int Quantity { get; set; }

        public int? QuantityUsed { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public EnumVoucherType VoucherType { get; set; }

        public bool IsActive { get; set; }

        public ICollection<VoucherPackage>? VoucherPackages { get; set; }
    }
}
