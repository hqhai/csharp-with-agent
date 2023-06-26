// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumParentErrorCode
    {
        /// <summary>
        /// Parent does not exist
        /// </summary>
        ParentNotExist,

        /// <summary>
        /// Parent is null
        /// </summary>
        ParentNull,

        /// <summary>
        /// Parent full name is not null
        /// </summary>
        ParentFullNameNotNull,

        /// <summary>
        /// Create student fail
        /// </summary>
        CreateStudentFail,

        /// <summary>
        /// Parent had more than 2 students
        /// </summary>
        ParentHadMoreTwoStudents,
    }
}
