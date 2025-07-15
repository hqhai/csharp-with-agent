// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumBadge
    {
        [Description("Hiệu suất đỉnh cao")]
        S,
        [Description("Hiệu suất ấn tượng")]
        A,
        [Description("Hiệu suất ổn định")]
        B,
        [Description("Bứt phá tiềm năng")]
        C,
        [Description("Cần cố gắng hơn")]
        D
    }
}
