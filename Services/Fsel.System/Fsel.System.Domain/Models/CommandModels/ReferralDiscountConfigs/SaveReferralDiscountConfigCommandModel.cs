// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ReferralDiscountConfigs
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SaveReferralDiscountConfigCommandModel : BaseCommandModel
    {
        public int IndexNumber { get; set; }

        public double? SenderDiscountValue { get; set; }

        public double? RecevierDiscountValue { get; set; }

        public EnumRecevicerDiscountType? RecevicerDiscountType { get; set; }

        public EnumSenderDiscountType? SenderDiscountType { get; set; }
    }
}
