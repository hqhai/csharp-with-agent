// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Voucher : Entity, IMultiLingualObject<VoucherTranslation>
    {
        /// <summary>
        /// Mã voucher
        /// </summary>
        [MaxLength(10, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Tiền tố
        /// </summary>
        [MaxLength(5, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CodePrefix { get; set; }

        /// <summary>
        /// Tên voucher
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(30, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Loại khuyễn mãi
        /// </summary>
        public EnumVoucherCategory VoucherCategory { get; set; }

        /// <summary>
        /// Giá trị khuyến mãi
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int Percent { get; set; }

        /// <summary>
        /// Số lượng tối đa
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int Quantity { get; set; }

        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Loại voucher
        /// </summary>
        public EnumVoucherType VoucherType { get; set; }

        /// <summary>
        /// Nguồn voucher
        /// </summary>
        public EnumVoucherSource Source { get; set; }

        /// <summary>
        /// Tên nguồn
        /// </summary>
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SourceName { get; set; }

        /// <summary>
        /// Id người được tặng
        /// </summary>
        public Guid? SourceUserId { get; set; }

        /// <summary>
        /// Event áp dụng
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? EventIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? EventIds
        {
            get { return ConvertHelper.Deserialize<IList<Guid>>(EventIdsStr); }
            set { EventIdsStr = ConvertHelper.Serialize(value); }
        }

        /// <summary>
        /// Trạng thái
        /// </summary>
        [NotMapped]
        public bool IsActive
        {
            get { return Shared.Helpers.DateTimeHelper.IsCurrentDateInRange(StartDate, EndDate); }
        }

        /// <summary>
        /// Cách sử dụng
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? DescriptionStr { get; set; }

        [NotMapped]
        public EventDescription? Description
        {
            get { return DescriptionStr.Deserialize<EventDescription>(); }
            set { DescriptionStr = value.Serialize(); }
        }

        /// <summary>
        /// Link Banner
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Banner { get; set; }

        public ICollection<VoucherPackage> VoucherPackages { get; set; } = new List<VoucherPackage>();

        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<VoucherTranslation> Translations { get; set; } = new List<VoucherTranslation>();
    }

    public class EventDescription
    {
        public IList<string>? HowToUses { get; set; }
        public IList<string>? Conditions { get; set; }
        public IList<string>? Contacts { get; set; }
        public IList<string>? Others { get; set; }
    }

    public class VoucherTranslation : Entity, ITranslationObject
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
