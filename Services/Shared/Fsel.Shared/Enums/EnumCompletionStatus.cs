// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumCompletionStatus
    {
        [Description("Hoàn thành")]
        Completed,

        [Description("Chưa hoàn thành")]
        InProgress,

        [Description("Chưa làm")]
        NotStarted
    }
}
