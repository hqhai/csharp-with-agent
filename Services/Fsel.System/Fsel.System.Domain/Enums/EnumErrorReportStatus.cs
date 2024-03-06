// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumErrorReportStatus
    {
        [Description("Chưa xử lý")]
        NotYetProcessed,

        [Description("Đã xử lý")]
        Processed,

        [Description("Đang xử lý")]
        Processing
    }
}
