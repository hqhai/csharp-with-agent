// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumStudentErrorCode
    {
        /// <summary>
        /// Student does not exist
        /// </summary>
        StudentNotExist,

        /// <summary>
        /// Student is null
        /// </summary>
        StudentNull,

        /// <summary>
        /// User is null
        /// </summary>
        UserNull,

        /// <summary>
        /// Students does not exist
        /// </summary>
        StudentsNotExist,

        /// <summary>
        /// Class with more than 12 students
        /// </summary>
        ClassMoreThan12Students,
    }
}
