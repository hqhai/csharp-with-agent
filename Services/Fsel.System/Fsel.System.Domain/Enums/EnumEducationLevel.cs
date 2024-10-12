// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumEducationLevel
    {
        [Description("Tiểu học")]
        Primary = 1,

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
