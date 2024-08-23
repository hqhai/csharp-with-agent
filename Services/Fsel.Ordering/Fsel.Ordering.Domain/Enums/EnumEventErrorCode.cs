// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumEventErrorCode
    {
        [Description("Thời gian bắt đầu lớn hơn thời gian kết thúc")]
        StartDateIsGreaterThanEndDate,

        [Description("Đã có sự kiện nằm trong khoảng thời gian này")]
        ThereWereEventsDuringThisTimePeriod,

        [Description("Sự kiện không tồn tại")]
        EventNotExist,

        [Description("Thiếu bản gi Package Event hoặc dịch")]
        MissingVersionOfPackageEventOrTranslation,

        [Description("Id Package Sai")]
        PackageIdIsWrong,

        [Description("Sự kiện đang hoạt động")]
        TheEventIsActive,

        [Description("Code đã tồn tại")]
        CodeIsAlreadyExist,

        [Description("Sự kiện đã hết hạn")]
        EventHasExpired
    }
}
