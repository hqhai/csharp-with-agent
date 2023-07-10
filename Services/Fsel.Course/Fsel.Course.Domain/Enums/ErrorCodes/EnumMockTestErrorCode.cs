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
        /// MockTest is not in active state
        /// </summary>
        MockTestNotInActiveState,

        /// <summary>
        /// User does not exist
        /// </summary>
        UserNotExist,

        /// <summary>
        /// MockTest must correct score
        /// </summary>
        MockTestMustCorrectScore,

        /// <summary>
        /// MockTest Type Does Not Exist
        /// </summary>
        MockTestTypeNotExist,

        /// <summary>
        /// Code And Level Already Exist
        /// </summary>
        CodeAndLevelAlreadyExist,

        /// <summary>
        /// MockTest Already Exist To ExtraPractice
        /// </summary>
        MockTestAlreadyExistToExtraPractice,

        /// <summary>
        /// MockTestId Not Null
        /// </summary>
        MockTestIdNotNull,

        /// <summary>
        /// Name Already Exists
        /// </summary>
        NameAlreadyExists

    }
}
