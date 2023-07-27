// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ReferralDiscountConfigModel : BaseModel
    {
        public int IndexNumber { get; set; }

        public double? SenderDiscountValue { get; set; }

        public double? RecevierDiscountValue { get; set; }

        public EnumRecevicerDiscountType? RecevicerDiscountType { get; set; }

        public EnumSenderDiscountType? SenderDiscountType { get; set; }
    }
}
