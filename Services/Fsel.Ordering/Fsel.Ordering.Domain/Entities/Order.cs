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
        public string? Code { get; set; }

        /// <summary>
        /// Tên Người dùng
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? FullName { get; set; }

        /// <summary>
        /// Email Người dùng
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// SDT Người dùng
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Quốc gia
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Country { get; set; }

        /// <summary>
        /// Địa chỉ
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
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
        public EnumPaymentMethodStatus PaymentMethod { get; set; }

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

        public Guid CourseId { get; set; }
        public Package? Package { get; set; }
        public Guid PackageId { get; set; }
        public Guid UserId { get; set; }
        public Guid ClassId { get; set; }
    }
}
