// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class Voucher : Entity
    {
        /// <summary>
        /// Mã voucher
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(8, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Tên voucher
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(30, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Phần trăm giảm
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
        public DateTime? EndDate { get; set; }

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
        /// Trạng thái
        /// </summary>
        [NotMapped]
        public bool IsActive
        {
            get { return Shared.Helpers.DateTimeHelper.IsCurrentDateInRange(StartDate, EndDate); }
        }

        public ICollection<VoucherPackage> VoucherPackages { get; set; } = new List<VoucherPackage>();

        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
