// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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
        public int Percent { get; set; }

        /// <summary>
        /// Số lượng tối đa
        /// </summary>
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
        /// Trạng thái
        /// </summary>
        public bool IsActive { get; set; }

        public ICollection<VoucherPackage> VoucherPackages { get; set; } = new List<VoucherPackage>();

        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
    }
}
