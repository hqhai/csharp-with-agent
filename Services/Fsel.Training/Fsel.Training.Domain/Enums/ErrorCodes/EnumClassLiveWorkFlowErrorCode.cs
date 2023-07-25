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
        /// Class Not Status Pedding
        /// </summary>
        ClassNotStatusPedding,

        /// <summary>
        /// ClassLiveWorkFlow Not Status AssignTeacher
        /// </summary>
        ClassLiveWorkFlowStatusAssignTeacher,

        /// <summary>
        /// Can Not cancel live time
        /// </summary>
        CanNotCancelLiveTime,

        /// <summary>
        /// Can Not assign teacher
        /// </summary>
        CanNotChangeTeacher,

        /// <summary>
        /// Can Not Change live time
        /// </summary>
        CanNotChangeLiveTime,

        /// <summary>
        /// live date is invalid
        /// </summary>
        LiveDateInvalid,

        /// <summary>
        /// ClassLiveWorkFlowPlans Null
        /// </summary>
        ClassLiveWorkFlowPlansNull,

        /// <summary>
        /// Voting time is not enough for 2 days
        /// </summary>
        VotingTimeIsNotEnoughForTwoDays
,
    }
}
