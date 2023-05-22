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
        /// MockTest Exists Other Than TypeSkillMockTest
        /// </summary>
        MockTestExistsOtherThanTypeSkillMockTest,

        /// <summary>
        /// MockTest Exists Other Than TypeFullMockTest
        /// </summary>
        MockTestExistsOtherThanTypeFullMockTest,

        /// <summary>
        /// MockTest  is in active state
        /// </summary>
        MockTestInActiveState,

        /// <summary>
        /// User does not exist
        /// </summary>
        UserNotExist

        /// <summary>
        /// MockTest must correct score
        /// </summary>
        MockTestMustCorrectScore
    }
}