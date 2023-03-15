// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseUnitMockTestErrorCode
    {
        /// <summary>
        /// Course Unit MockTest does not exist
        /// </summary>
        CUM01V,

        /// <summary>
        /// Course Unit MockTest have been use
        /// </summary>
        CUM02V,

        /// <summary>
        /// Course Unit MockTest null
        /// </summary>
        CUM03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        CUM01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        CUM02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        CUM03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        CUM04C,
    }
}
