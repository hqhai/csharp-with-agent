// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Ordering.Domain.Entities
{
    public class Order : Entity
    {
        /// <summary>
        /// Code Order
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Tên Người dùng
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FullName { get; set; }

        /// <summary>
        /// Email Người dùng
        /// </summary>
        [MaxLength(70, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Email { get; set; }

        /// <summary>
        /// SDT Người dùng
        /// </summary>
        [MaxLength(20, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Địa chỉ
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Address { get; set; }

        /// <summary>
        /// Id tỉnh, thành phố
        /// </summary>
        public Guid? ProvinceId { get; set; }

        /// <summary>
        /// Id quận, huyện
        /// </summary>
        public Guid? DistrictId { get; set; }

        /// <summary>
        /// Trạng thái Đơn hàng
        /// </summary>
        public EnumOrderStatus Status { get; set; }

        ///<summary>
        /// Phương thức thanh toán
        ///</summary>
        public EnumPaymentMethodStatus? PaymentMethod { get; set; }

        ///<summary>
        /// Loại thanh toán
        ///</summary>
        public EnumPaymentRevenueType? RevenueType { get; set; }

        /// <summary>
        /// Giá Khóa Học
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal Price { get; set; }

        /// <summary>
        /// Giảm giá %
        /// </summary>
        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int DiscountPercent { get; set; }

        /// <summary>
        /// Tổng tiền Giảm giá %
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal DiscountPrice { get; set; }

        /// <summary>
        /// Thành tiền tổng
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Thời gian Học
        /// </summary>
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// Học thử
        /// </summary>
        public bool IsTrial { get; set; }

        public Package? Package { get; set; }

        /// <summary>
        /// Id Package
        /// </summary>
        public Guid? PackageId { get; set; }

        public Event? Event { get; set; }

        /// <summary>
        /// Id Sự kiện
        /// </summary>
        public Guid? EventId { get; set; }

        public Guid UserId { get; set; }

        /// <summary>
        /// Mã giới thiệu
        /// </summary>
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ReferralCode { get; set; }

        #region Company invoice information

        /// <summary>
        /// Có xuất hóa đơn hay không
        /// </summary>
        public bool IsInvoice { get; set; }

        /// <summary>
        /// Tên công ty
        /// </summary>
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CompanyName { get; set; }

        /// <summary>
        /// Địa chỉ công ty
        /// </summary>
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CompanyAddress { get; set; }

        /// <summary>
        /// Mã số thuế
        /// </summary>
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CompanyTaxCode { get; set; }

        [MaxLength(70, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? CompanyEmail { get; set; }

        #endregion Company invoice information

        #region Không sử dụng

        /// <summary>
        /// Không sử dụng
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// Không sử dụng
        /// </summary>
        public Guid? ClassId { get; set; }

        #endregion Không sử dụng

        public Guid? VoucherId { get; set; }
        public Voucher? Voucher { get; set; }

        public ICollection<OrderTransaction> OrderTransactions { get; set; } = new List<OrderTransaction>();
    }
}
