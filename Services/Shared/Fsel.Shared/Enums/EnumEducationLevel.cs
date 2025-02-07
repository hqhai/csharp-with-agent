// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumEducationLevel
    {
        [Description("Mầm non")]
        Preschool,

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
