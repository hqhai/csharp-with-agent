// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumCrmLocationTypeLevel
    {
        [Description("Không xác định")]
        Unknown,

        [Description("Tiểu học")]
        Primary,

        [Description("THCS")]
        Secondary,

        [Description("THPT")]
        HighSchool,

        [Description("Liên cấp")]
        InterLevel,

        [Description("Đại học")]
        University
    }
}
