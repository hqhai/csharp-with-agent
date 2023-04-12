// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonErrorCode
    {
        /// <summary>
        /// Lesson does not exist
        /// </summary>
        LessonNotExist,

        /// <summary>
        /// LessonId not exist
        /// </summary>
        LessonIdNotExist,

        /// <summary>
        /// Lesson was used
        /// </summary>
        LessonUsed,

        /// <summary>
        /// Lesson not is correct
        /// </summary>
        LessonNotCorrect,

        /// <summary>
        /// User Not Student
        /// </summary>
        NotStudent,

        /// <summary>
        /// Student not in class
        /// </summary>
        StudentNotInClass,

        /// <summary>
        /// Student null
        /// </summary>
        StudentNull,

        /// <summary>
        /// LessonIds null
        /// </summary>
        LessonIdsNull,

        /// <summary>
        /// Lesson does not exist
        /// </summary>
        LessonIdsNotExist
    }
}
