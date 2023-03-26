namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseErrorCode
    {
        /// <summary>
        /// Course does not exist
        /// </summary>
        CourseNotExist,

        /// <summary>
        /// List Course does not exist
        /// </summary>
        ListCourseNotExist,

        /// <summary>
        /// Course is not in a new state
        /// </summary>
        CourseNotInNewState,

        /// <summary>
        /// User Not Student
        /// </summary>
        NotStudent,

        /// <summary>
        /// Student not in class
        /// </summary>
        StudentNotInClass,

        /// <summary>
        /// List Teacher by course does not exist
        /// </summary>
        ListTeacherCourseNotExist,

        /// <summary>
        /// Course not in Class
        /// </summary>
        CourseNotInClass,

        /// <summary>
        /// Classes new not exist
        /// </summary>
        ClassesNewNotExitst,

        /// <summary>
        /// Students not exist
        /// </summary>
        StudentsNotExist,
    }
}
