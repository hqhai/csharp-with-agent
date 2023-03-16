// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseTeacherErrorCode
    {
        /// <summary>
        /// CourseTeacher does not exist
        /// </summary>
        CT01V,

        /// <summary>
        /// CourseTeacher have been use
        /// </summary>
        CT02V,

        /// <summary>
        /// mCourseTeacher null
        /// </summary>
        CT03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        CT01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        CT02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        CT03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        CT04C,
    }
}
