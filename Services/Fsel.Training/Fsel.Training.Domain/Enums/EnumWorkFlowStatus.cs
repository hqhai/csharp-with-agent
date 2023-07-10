// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Enums
{
    public enum EnumWorkFlowChangeTeacherStatus
    {
        RequestChangeTeacher,
        WaitConfirm,
        DoneScheduled
    }

    public enum EnumWorkFlowAssignTeacherStatus
    {
        Pending,
        Approved,
        Reject
    }

    public enum EnumWorkFlowCancelScheduleStatus
    {
        RequestCancel,
        WaitVote,
        DoneScheduled
    }
}
