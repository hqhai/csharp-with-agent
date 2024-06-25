// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumEducationLevel
    {
        [Description("Tiểu học")]
        Primary,

        [Description("THCS")]
        Secondary,

        [Description("THPT")]
        HighSchool,

        [Description("Đại học")]
        University,

        [Description("Cao đẳng")]
        College,

        [Description("Liên Cấp")]
        InterLevel
    }
}
