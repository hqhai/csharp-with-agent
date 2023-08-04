// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class ReferralDiscountConfig : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [Range(1, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int IndexNumber { get; set; }

        public double? SenderDiscountValue { get; set; }

        public double? RecevierDiscountValue { get; set; }

        public EnumDiscountType? RecevicerDiscountType { get; set; }

        public EnumDiscountType? SenderDiscountType { get; set; }
    }
}
