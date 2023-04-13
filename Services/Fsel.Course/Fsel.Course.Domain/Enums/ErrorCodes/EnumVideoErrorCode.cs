// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoErrorCode
    {
        //    /// <summary>
        //    /// Video does not exist
        //    /// </summary>
        //    VideoNotExist,

        /// <summary>
        /// VideoIds not exist
        /// </summary>
        VideoIdsNotExist,

        /// <summary>
        /// Video was used
        /// </summary>
        VideoUsed,

        /// <summary>
        /// VideoId Not Exist
        /// </summary>
        VideoIdNotExist,

        /// <summary>
        /// VideoIds Null
        /// </summary>
        VideoIdsNull,

        //    /// <summary>
        //    /// TeacherId does not exist
        //    /// </summary>
        //    TeacherIdDoesNotExitst,

        /// <summary>
        /// Config is in the wrong format
        /// </summary>
        ConfigIsInTheWrongFormat,

        /// <summary>
        /// Cannot have both UnitTest and SkillTest at the same time
        /// </summary>
        CanNotUnitTestAndSkillTestAtTheSameTime
    }
}
