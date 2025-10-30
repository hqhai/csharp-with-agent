// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumLanguage
    {
        /// <summary>Tiếng Việt</summary>
        [Description("vi-VN")]
        Vietnamese = 1,

        /// <summary>Tiếng Anh</summary>
        [Description("en-US")]
        English = 2,

        /// <summary>Tiếng Nhật</summary>
        [Description("ja-JP")]
        Japanese = 3,

        /// <summary>Tiếng Hàn</summary>
        [Description("ko-KR")]
        Korean = 4,

        /// <summary>Tiếng Trung (Giản thể)</summary>
        [Description("zh-CN")]
        Chinese = 5,

        /// <summary>Tiếng Pháp</summary>
        [Description("fr-FR")]
        French = 6
    }
}
