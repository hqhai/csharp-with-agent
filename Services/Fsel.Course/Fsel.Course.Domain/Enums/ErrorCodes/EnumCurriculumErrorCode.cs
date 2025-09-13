// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCurriculumErrorCode
    {
        /// <summary>
        /// Ngày bắt đầu lớn hơn ngày kết thúc
        /// </summary>
        InvalidDateRange,

        /// <summary>
        /// Tồn tại giáo trình với tên đã nhập
        /// </summary>
        AlreadyExistCurriculumName,

        /// <summary>
        /// Giáo trình đã được kích hoạt
        /// </summary>
        AlreadyActiveCurriculum,

        /// <summary>
        /// Ngày bắt đầu nhỏ hơn ngày hiện tại
        /// </summary>
        StartDateCannotBeInThePast,

        /// <summary>
        /// Không tìm thấy giáo trình
        /// </summary>
        CurriculumDoesNotExist
    }
}
