// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseErrorCode
    {
        /// <summary>
        /// Courses not exist
        /// </summary>
        CoursesNotExist,

        /// <summary>
        /// CourseId does not exist
        /// </summary>
        CourseIdNotExist,

        ///// <summary>
        ///// List Course does not exist
        ///// </summary>
        //ListCourseNotExist,

        /// <summary>
        /// Course is not in a new state
        /// </summary>
        CourseNotInNewState,

        ///// <summary>
        ///// User Not Student
        ///// </summary>
        //NotStudent,

        /// <summary>
        /// Student not in class
        /// </summary>
        StudentNotInClass,

        /// <summary>
        /// TeacherIds Not Exist
        /// </summary>
        TeacherIdsNotExist,

        ///// <summary>
        ///// Course not in Class
        ///// </summary>
        //CourseNotInClass,

        ///// <summary>
        ///// Classes new not exist
        ///// </summary>
        //ClassesNewNotExitst,

        ///// <summary>
        ///// Students not exist
        ///// </summary>
        //StudentsNotExist,

        /// <summary>
        /// Classe Id not exist
        /// </summary>
        ClasseIdNotExist,

        /// <summary>
        /// Classe Code not exist
        /// </summary>
        ClasseCodeNotExist,

        /// <summary>
        /// Student null
        /// </summary>
        StudentNull,

        /// <summary>
        /// MocktestId and UnitId cannot have values at the same time.
        /// </summary>
        MocktestIdAndUnitIdAreMutuallyExclusive,

        /// <summary>
        /// Course must be in an active state
        /// </summary>
        CourseMustActiveState
    }
}
