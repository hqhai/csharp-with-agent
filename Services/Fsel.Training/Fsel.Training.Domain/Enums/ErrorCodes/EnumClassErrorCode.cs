// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Enums.ErrorCodes
{
    public enum EnumClassErrorCode
    {
        /// <summary>
        /// Class has too many students
        /// </summary>
        ClassHasTooManyStudents,

        /// <summary>
        /// Delete Class In Student Not Success
        /// </summary>
        DeleteClassInStudentNotSuccess,

        /// <summary>
        /// Status Of Class is not New
        /// </summary>
        StatusOfClassIsNotNew,

        /// <summary>
        /// Course Time not installed
        /// </summary>
        CourseTimeNotInstalled,

        /// <summary>
        /// Student not exist in class
        /// </summary>
        StudentNotExistInClass,

        /// <summary>
        /// Student Is Already In The Class
        /// </summary>
        StudentIsAlreadyInTheClass,

        /// <summary>
        /// Class Already Has Cso
        /// </summary>
        ClassAlreadyHasCso,

        /// <summary>
        /// Cso Already In Class
        /// </summary>
        CsoAlreadyInClass,

        /// <summary>
        /// Teacher Is Already In The Class
        /// </summary>
        TeacherIsAlreadyInTheClass,

        /// <summary>
        /// Teacher Approve Status Is Approve
        /// </summary>
        TeacherApproveStatusIsApprove,

        /// <summary>
        /// Class End Date And Start Date Is Null
        /// </summary>
        ClassEndDateAndStartDateIsNull,

        /// <summary>
        /// LiveTimeFrame Null Or LiveDays Null
        /// </summary>
        LiveTimeFrameNullOrLiveDaysNull,

        /// <summary>
        /// Orders Not Approved
        /// </summary>
        OrdersNotApproved

,
    }
}
