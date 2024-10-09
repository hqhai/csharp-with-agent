// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using Fsel.Common.Helpers;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Core.Entities;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;

    public class Product : Entity, IMultiLingualObject<ProductTranslation>
    {
        /// <summary>
        /// Code của gift
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(25, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Name của gift
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Số lượng quà
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int Quantity { get; set; }

        /// <summary>
        /// Ưu tiên hiển thị
        /// </summary>
        [DefaultValue(true)]
        public bool ShowPriority { get; set; }

        /// <summary>
        /// Giá quà
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int Price { get; set; }

        /// <summary>
        /// Ngày hết hạn đổi quà
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumProductStatus Status { get; set; }

        /// <summary>
        /// link ảnh quà
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ImagesStr { get; set; }

        [NotMapped]
        public IList<string>? Images
        {
            get { return ImagesStr.Deserialize<IList<string>>(); }
            set { ImagesStr = value.Serialize(); }
        }

        /// <summary>
        /// Id các sự kiện áp dụng
        /// </summary>
        public string? EventIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? EventIds
        {
            get { return EventIdsStr.Deserialize<IList<Guid>>(); }
            set { EventIdsStr = value.Serialize(); }
        }

        /// <summary>
        /// Cách sử dụng
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? DescriptionStr { get; set; }

        [NotMapped]
        public ProductDescription? Description
        {
            get { return DescriptionStr.Deserialize<ProductDescription>(); }
            set { DescriptionStr = value.Serialize(); }
        }

        public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();
        public ICollection<OrderTransaction> OrderTransactions { get; set; } = new List<OrderTransaction>();
    }

    public class ProductDescription
    {
        public IList<string>? HowToUses { get; set; }
        public IList<string>? Conditions { get; set; }
        public IList<string>? Contacts { get; set; }
        public IList<string>? Others { get; set; }
    }

    public class ProductTranslation : Entity, ITranslationObject
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? DescriptionStr { get; set; }

        [NotMapped]
        public ProductDescription? Description
        {
            get { return DescriptionStr.Deserialize<ProductDescription>(); }
            set { DescriptionStr = value.Serialize(); }
        }

        public Guid ProductId { get; set; }

        public Product? Product { get; set; }

        public string? Language { get; set; }
    }
}
