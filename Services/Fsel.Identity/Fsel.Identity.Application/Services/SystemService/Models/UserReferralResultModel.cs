// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UserReferralResultModel : BaseModel
    {
        public int IndexNumber { get; set; }
        public double? SenderDiscountValue { get; set; }
        public double? RecevierDiscountValue { get; set; }
        public EnumDiscountType? RecevicerDiscountType { get; set; }
        public EnumDiscountType? SenderDiscountType { get; set; }
    }
}
