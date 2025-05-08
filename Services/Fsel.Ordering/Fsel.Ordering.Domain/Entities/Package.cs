// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Package : Entity, IMultiLingualObject<PackageTranslation>
    {
        /// <summary>
        /// Code
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public EnumPackageCode? Code { get; set; }

        /// <summary>
        /// Name
        /// </summary>

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Giá Khóa Học
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal Price { get; set; }

        /// <summary>
        /// Giá trên tháng
        /// </summary>

        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal PriceMonth { get; set; }

        /// <summary>
        /// Thời gian Khóa Học
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int MonthNumber { get; set; }

        /// <summary>
        /// Gợi ý
        /// </summary>

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? IncentivesWhenPurchasing { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// Coin tặng chức năng mời bạn bè
        /// </summary>
        [Range(0, 240000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int ReferToken { get; set; }

        /// <summary>
        /// Coin tặng khi mua khóa học
        /// </summary>
        [Range(0, 240000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int BonusCoins { get; set; }

        public EnumPackageStatus Status { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<VoucherPackage> VoucherPackages { get; set; } = new List<VoucherPackage>();

        public ICollection<PackageTranslation> Translations { get; set; } = new List<PackageTranslation>();

        public ICollection<PackageEvent> PackageEvents { get; set; } = new List<PackageEvent>();
    }

    public class PackageTranslation : Entity, ITranslationObject
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? IncentivesWhenPurchasing { get; set; }

        public Guid PackageId { get; set; }

        public Package? Package { get; set; }

        public string? Language { get; set; }
    }
}
