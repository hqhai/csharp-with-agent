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
        /// Student is null
        /// </summary>
        StudentNull,

        /// <summary>
        /// List Lesson does not exist
        /// </summary>
        ListLessonNotExist,
    }
}
