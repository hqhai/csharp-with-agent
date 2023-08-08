// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ReferralDiscountConfigs
{
    using Fsel.Shared.Enums;

    public class SaveListReferralDiscountConfigCommandModel
    {
        public IList<SaveReferralDiscountConfigCommandModel>? SaveReferralDiscountConfigs { get; set; }
    }

    public class SaveReferralDiscountConfigCommandModel
    {
        public int IndexNumber { get; set; }

        public double? SenderDiscountValue { get; set; }

        public double? RecevierDiscountValue { get; set; }

        public EnumDiscountType? RecevicerDiscountType { get; set; }

        public EnumDiscountType? SenderDiscountType { get; set; }
    }
}
