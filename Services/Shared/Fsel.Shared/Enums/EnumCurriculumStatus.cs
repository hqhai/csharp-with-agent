// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumCurriculumStatus
    {
        [Description("Đang triển khai")]
        Progress,

        [Description("Đã hết hạn")]
        Expired,

        [Description("Chưa triển khai")]
        NotProgress
    }
}
