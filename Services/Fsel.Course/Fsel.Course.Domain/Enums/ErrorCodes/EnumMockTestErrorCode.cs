// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumMockTestErrorCode
    {
        /// <summary>
        /// MockTests does not exist
        /// </summary>
        MockTestsNotExist,

        /// <summary>
        /// MockTest Exists Other Than TypeUnitMockTest
        /// </summary>
        MockTestExistsOtherThanTypeUnitMockTest,

        /// <summary>
        /// MockTest Exists Other Than TypeCourseMocktest
        /// </summary>
        MockTestExistsOtherThanTypeCourseMocktest,

        /// <summary>
        /// MockTest  is in active state
        /// </summary>
        MockTestInActiveState,
    }
}
