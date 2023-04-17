// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoErrorCode
    {
        /// <summary>
        /// VideoIds does not exist
        /// </summary>
        VideoIdsNotExist,

        /// <summary>
        /// Video was used
        /// </summary>
        VideoUsed,

        /// <summary>
        /// VideoId does not exist
        /// </summary>
        VideoIdNotExist,

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
        /// UserId Not Exist
        /// </summary>
        UserIdNotExist
    }
}
