// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumCombinedProgress
    {
        [Description("Vượt tiến độ tổng, vượt tiến độ tuần")]
        TotalAheadWeekAhead,

        [Description("Vượt tiến độ tổng, đúng tiến độ tuần")]
        TotalAheadWeekOnTrack,

        [Description("Vượt tiến độ tổng, chậm tiến độ tuần")]
        TotalAheadWeekBehind,

        // Tổng: OnTrack/Đúng
        [Description("Đúng tiến độ tổng, vượt tiến độ tuần")]
        TotalOnTrackWeekAhead,

        [Description("Đúng tiến độ tổng, đúng tiến độ tuần")]
        TotalOnTrackWeekOnTrack,

        [Description("Đúng tiến độ tổng, chậm tiến độ tuần")]
        TotalOnTrackWeekBehind,

        // Tổng: Behind/Chậm
        [Description("Chậm tiến độ tổng, vượt tiến độ tuần")]
        TotalBehindWeekAhead,

        [Description("Chậm tiến độ tổng, đúng tiến độ tuần")]
        TotalBehindWeekOnTrack,

        [Description("Chậm tiến độ tổng, chậm tiến độ tuần")]
        TotalBehindWeekBehind
    }
}
