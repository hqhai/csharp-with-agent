// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoTimeCodeErrorCode
    {
        /// <summary>
        /// Number of DisplayTime >1
        /// </summary>
        DisplayTimeGreaterThan1,

        /// <summary>
        /// Video Time Code does not exist
        /// </summary>
        VideoTimeCodeNotExist,

        /// <summary>
        /// Video Time Code Id does not exist
        /// </summary>
        VideoTimeCodeIdNotExist,

        /// <summary>
        /// Video Time Codes is null
        /// </summary>
        VideoTimeCodesNull,

        /// <summary>
        /// Video Time Code is null
        /// </summary>
        VideoTimeCodeNull
    }
}
