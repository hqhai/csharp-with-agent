// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonErrorCode
    {
        /// <summary>
        /// LessonIds does not exist
        /// </summary>
        LessonIdsNotExist,

        /// <summary>
        /// LessonId does not exist
        /// </summary>
        LessonIdNotExist,

        /// <summary>
        /// Lesson was used
        /// </summary>
        LessonUsed,

        /// <summary>
        /// LessonIds is null
        /// </summary>
        LessonIdsNull,

        /// <summary>
        /// Lessons does not exist
        /// </summary>
        LessonsNotExist,

        /// <summary>
        /// UserId does not exist
        /// </summary>
        UserIdNotExist
    }
}
