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
        /// Videos name is exist
        /// </summary>
        VideoNameIsExist,

        /// <summary>
        /// Video was used
        /// </summary>
        VideoUsed,

        /// <summary>
        /// Video does not exist
        /// </summary>
        VideoNotExist,

        /// <summary>
        /// Videos Is Null
        /// </summary>
        VideosNull,

        /// <summary>
        /// Config is in the wrong format
        /// </summary>
        ConfigIsInTheWrongFormat,

        /// <summary>
        /// Cannot have both UnitTest and SkillTest at the same time
        /// </summary>
        CanNotUnitTestAndSkillTestAtTheSameTime,

        /// <summary>
        /// User Does Not Exist
        /// </summary>
        UserNotExist
    }
}
