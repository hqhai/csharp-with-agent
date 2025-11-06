// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumChartType
    {
        [Description("Biểu đồ cột")]
        BarChart = 1,

        [Description("Biểu đồ đường")]
        LineChart = 2,

        [Description("Biểu đồ tròn")]
        PieChart = 3,

        [Description("Biểu đồ cột chồng")]
        StackbarChart = 4
    }
}
