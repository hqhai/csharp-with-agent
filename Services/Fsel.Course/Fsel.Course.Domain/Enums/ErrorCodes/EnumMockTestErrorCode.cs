// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumMockTestErrorCode
    {
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
        /// MockTest must correct score
        /// </summary>
        MockTestMustCorrectScore,

        /// <summary>
        /// Skill Speaking Or Writing
        /// </summary>
        SkillSpeakingOrWriting,

        /// <summary>
        /// Cannot initialize a version smaller than the current version
        /// </summary>
        VersionTooLow,

        /// <summary>
        /// BelowOrEqualTo40
        /// </summary>
        BelowOrEqualTo40
    }
}
