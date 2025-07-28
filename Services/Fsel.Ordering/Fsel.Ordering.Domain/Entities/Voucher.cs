// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Voucher : Entity, IMultiLingualObject<VoucherTranslation>
    {
        public Voucher Clone()
        {
            return (Voucher)MemberwiseClone();
        }

        /// <summary>
        /// Mã voucher
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
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
        [MaxLength(80, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Loại khuyễn mãi
        /// </summary>
        public EnumVoucherCategory Category { get; set; }

        /// <summary>
        /// Giá trị khuyến mãi
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int Value { get; set; }

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
        /// Đối tượng áp dụng
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ApplicableSubjectsStr { get; set; }

        [NotMapped]
        public IList<EnumApplicableSubjectsVoucher>? ApplicableSubjects
        {
            get { return ConvertHelper.Deserialize<IList<EnumApplicableSubjectsVoucher>>(ApplicableSubjectsStr); }
            set { ApplicableSubjectsStr = ConvertHelper.Serialize(value); }
        }

        /// <summary>
        /// Link file chứa email
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ExcelFilePath { get; set; }

        public string? ApplicableEmailsStr { get; set; }

        [NotMapped]
        public IList<string>? ApplicableEmails
        {
            get { return ConvertHelper.Deserialize<IList<string>>(ApplicableEmailsStr); }
            set { ApplicableEmailsStr = ConvertHelper.Serialize(value); }
        }

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

        [DefaultValue(false)]
        public bool IsShowMyVoucher { get; set; }

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
        public VoucherDescription? Description
        {
            get { return DescriptionStr.Deserialize<VoucherDescription>(); }
            set { DescriptionStr = value.Serialize(); }
        }

        /// <summary>
        /// Số lượt đổi
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? NumberOfChanges { get; set; }

        /// <summary>
        /// Link Banner
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Banner { get; set; }

        public ICollection<VoucherPackage> VoucherPackages { get; set; } = new List<VoucherPackage>();

        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        /// <summary>
        /// Đa ngôn ngữ
        /// </summary>
        public string? TranslationsStr { get; set; }

        [NotMapped]
        public ICollection<VoucherTranslation>? Translations
        {
            get { return TranslationsStr.Deserialize<ICollection<VoucherTranslation>>(); }
            set { TranslationsStr = value.Serialize(); }
        }
    }

    public class VoucherDescription
    {
        public IList<string>? Title { get; set; }
        public IList<string>? HowToUses { get; set; }
        public IList<string>? Conditions { get; set; }
        public IList<string>? Contacts { get; set; }
        public IList<string>? Others { get; set; }
    }

    public class VoucherTranslation : ITranslationObject
    {
        public string? Name
        { get { return Description?.Title?.FirstOrDefault(); } }

        public string? DescriptionStr { get; set; }

        [NotMapped]
        public VoucherDescription? Description
        {
            get { return DescriptionStr.Deserialize<VoucherDescription>(); }
            set { DescriptionStr = value.Serialize(); }
        }

        public string? Language { get; set; }
    }
}
