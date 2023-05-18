// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumPlacementTestErrorCode
    {
        /// <summary>
        /// Placement Test does not exist
        /// </summary>
        PlacementTestNotExist,

        /// <summary>
        /// Placement Test is null
        /// </summary>
        PlacementTestNull,

        /// <summary>
        /// Placement Test  is in active state
        /// </summary>
        PlacementTestInActiveState,

        /// <summary>
        /// PlacementTest must have the correct score
        /// </summary>
        PlacementTestMustCorrectScore,

        /// <summary>
        /// Call to service UserService error
        /// </summary>
        CallUserServiceError
    }
}
