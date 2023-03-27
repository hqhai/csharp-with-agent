namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseErrorCode
    {
        /// <summary>
        /// Course does not exist
        /// </summary>
        CourseNotExist,

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
        /// Student is null
        /// </summary>
        StudentNull,
    }
}
