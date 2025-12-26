// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumPracticeProgressStatus
    {
        [Description("Đề mới")]
        NotStarted,

        [Description("Đang làm bài")]
        InProgress,

        [Description("Đã hoàn thành")]
        Completed
    }
}
