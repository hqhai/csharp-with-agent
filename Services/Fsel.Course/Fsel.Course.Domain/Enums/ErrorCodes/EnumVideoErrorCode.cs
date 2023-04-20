// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoErrorCode
    {
        /// <summary>
        /// Videos does not exist
        /// </summary>
        VideosNotExist,

        /// <summary>
        /// Video was used
        /// </summary>
        VideoUsed,

        /// <summary>
        /// Video does not exist
        /// </summary>
        VideoNotExist,

        /// <summary>
        /// VideoIds is null
        /// </summary>
        VideoIdsNull,

        /// <summary>
        /// Config is in the wrong format
        /// </summary>
        ConfigIsInTheWrongFormat,

        /// <summary>
        /// Cannot have both UnitTest and SkillTest at the same time
        /// </summary>
        CanNotUnitTestAndSkillTestAtTheSameTime,

        /// <summary>
        /// User Not Exist
        /// </summary>
        UserNotExist
    }
}
