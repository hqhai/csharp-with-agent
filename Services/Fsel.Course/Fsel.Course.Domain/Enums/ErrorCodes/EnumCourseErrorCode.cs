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
        /// CourseId does not exist
        /// </summary>
        CourseIdNotExist,

        /// <summary>
        /// CourseId already exists
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
        /// TeacherIds does not exist
        /// </summary>
        TeacherIdsNotExist,

        /// <summary>
        /// Classe Id does not exist
        /// </summary>
        ClasseIdNotExist,

        /// <summary>
        /// Classes does not exist
        /// </summary>
        ClassesNotExist,

        /// <summary>
        /// Classe Code does not exist
        /// </summary>
        ClasseCodeNotExist,

        /// <summary>
        /// Student is null
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
