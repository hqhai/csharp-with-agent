// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumStatusStudentGoal
    {
        [Description("Bình thường")]
        Normal,
        [Description("Gọi điện lần 1")]
        CallFirstTime,
        [Description("Gọi điện lần 2")]
        CallSecondTime,
        [Description("Gọi điện lần 3")]
        CallThirdTime,
        [Description("Gặp mặt lần 1")]
        MeetFirstTime,
        [Description("Gặp mặt lần 2")]
        MeetSecondTime,
        [Description("Gặp mặt lần 3")]
        MeetThirdTime,
        [Description("Đình chỉ")]
        Suspend,
        [Description("Chưa đóng tiền")]
        Unpaid,
    }
}
