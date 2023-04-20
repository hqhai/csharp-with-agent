// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseErrorCode
    {
        /// <summary>
        /// Courses does not exist
        /// </summary>
        CoursesNotExist,

        /// <summary>
        /// Course does not exist
        /// </summary>
        CourseNotExist,

        /// <summary>
        /// Course already exists
        /// </summary>
        CourseCodeIsExist,

        /// <summary>
        /// Course is not in a new state
        /// </summary>
        CourseNotInNewState,

        /// <summary>
        /// Student not in class
        /// </summary>
        StudentNotInClass,

        /// <summary>
        /// Teachers does not exist
        /// </summary>
        TeachersNotExist,

        /// <summary>
        /// Class does not exist
        /// </summary>
        ClassNotExist,

        /// <summary>
        /// Classes does not exist
        /// </summary>
        ClassesNotExist,

        /// <summary>
        /// Classe Code does not exist
        /// </summary>
        ClasseCodeNotExist,

        /// <summary>
        /// Student Is Null
        /// </summary>
        StudentNull,

        /// <summary>
        /// MocktestId and UnitId cannot have values at the same time.
        /// </summary>
        MocktestIdAndUnitIdAreMutuallyExclusive,

        /// <summary>
        /// Course must be in an Active state
        /// </summary>
        CourseMustActiveState,

        /// <summary>
        /// Course is in Active state
        /// </summary>
        CourseIsActiveState,

        /// <summary>
        /// Course is in New state, can't start Lesson
        /// </summary>
        CourseIsNewStateCantStartLesson,
    }
}
