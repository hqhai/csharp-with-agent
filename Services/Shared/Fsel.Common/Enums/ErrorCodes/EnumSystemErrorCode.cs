// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Enums.ErrorCodes
{
    public enum EnumSystemErrorCode
    {
        /// <summary>
        /// Server Error
        /// Lỗi hệ thống
        /// </summary>
        ServerError,

        /// <summary>
        /// Field {0} cannot be empty
        /// Thông tin {0} không được để trống
        /// </summary>
        Required,

        /// <summary>
        /// Field {0} limited to {1} characters
        /// Thông tin {0} giới hạn {1} ký tự
        /// </summary>
        MaxLength,

        /// <summary>
        /// Field {0} cannot be less than {1}
        /// Thông tin {0} không thể nhỏ hơn {1}
        /// </summary>
        Min,
    }
}
