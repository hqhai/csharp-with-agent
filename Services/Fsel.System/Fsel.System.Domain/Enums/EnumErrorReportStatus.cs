// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumErrorReportStatus
    {
        [Description("Chưa xử lý")]
        New,

        [Description("Đã xử lý")]
        Done,

        [Description("Đang xử lý")]
        Process
    }
}
