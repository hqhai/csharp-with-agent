// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Enums.ErrorCodes
{
    public enum EnumClassLiveWorkFlowErrorCode
    {
        /// <summary>
        /// ClassLiveWorkFlow Does not exits
        /// </summary>
        ClassLiveWorkFlowNotExits,

        /// <summary>
        /// Teacher Does not exits
        /// </summary>
        TeacherNotExits,

        /// <summary>
        /// CSO Does not exits
        /// </summary>
        CSONotExits,

        /// <summary>
        /// ClassLiveWorkFlow Already exist
        /// </summary>
        ClassLiveWorkFlowAlreadyExist,

        /// <summary>
        /// Class Not Status Approved
        /// </summary>
        ClassNotStatusApproved,

        /// <summary>
        /// ClassLiveWorkFlow Not Status AssignTeacher
        /// </summary>
        ClassLiveWorkFlowStatusAssignTeacher
    }
}
