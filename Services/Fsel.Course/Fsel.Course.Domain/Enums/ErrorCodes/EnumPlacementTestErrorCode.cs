// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumPlacementTestErrorCode
    {
        /// <summary>
        /// Placement Test  is in active state
        /// </summary>
        PlacementTestInActiveState,

        /// <summary>
        ///  must have the correct score
        /// </summary>
        MustCorrectScore,

        /// <summary>
        ///  must have the correct score
        /// </summary>
        PlacementTestResultMaxThree,

        /// <summary>
        ///  PlacementTest Lock
        /// </summary>
        PlacementTestLock,

        /// <summary>
        ///  PlacementTest Done
        /// </summary>
        PlacementTestDone,

        /// <summary>
        ///  You chose the wrong Level
        /// </summary>
        YouChoseTheWrongLevel,

        /// <summary>
        /// SectionGroupResult Done
        /// </summary>
        SectionGroupResultDone,
    }
}
