// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumProgressStatus
    {
        [Description("Vượt tiến độ")]
        Ahead,

        [Description("Đúng tiến độ")]
        OnTrack,

        [Description("Chậm tiến độ")]
        Behind,
    }
}
