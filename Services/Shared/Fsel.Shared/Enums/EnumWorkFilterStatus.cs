// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumWorkFilterStatus
    {
        [Description("Chưa hoàn thành")]
        NotCompleted = 0,

        [Description("Đã hoàn thành")]
        Completed = 1,

        [Description("Quá hạn")]
        Overdue = 2
    }
}