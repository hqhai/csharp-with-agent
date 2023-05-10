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
        /// Unit have been use
        /// </summary>
        UnitUsed,

        /// <summary>
        /// Course have been use
        /// </summary>
        CourseUsed,
    }
}
