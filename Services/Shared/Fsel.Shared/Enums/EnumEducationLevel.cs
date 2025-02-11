// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumEducationLevel
    {
        [Description("Mầm non")]
        Preschool = 1,

        [Description("Tiểu học")]
        Primary,

        [Description("THCS")]
        Secondary,

        [Description("THPT")]
        HighSchool,

        [Description("Liên Cấp")]
        InterLevel,

        [Description("Đại học")]
        University
    }
}
