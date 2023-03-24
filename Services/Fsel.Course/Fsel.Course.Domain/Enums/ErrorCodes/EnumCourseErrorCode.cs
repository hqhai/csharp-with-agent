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
        /// List Teacher by course does not exist
        /// </summary>
        ListTeacherCourseNotExist,

        /// <summary>
        /// Course not in Class
        /// </summary>
        CourseNotInClass,
    }
}
