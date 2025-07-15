// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    /// <summary>
    /// Entity lưu những từ không tìm thấy trong từ điển Anh - Việt
    /// </summary>
    public class UnknownWord : Entity
    {
        /// <summary>
        /// Từ tiếng anh
        /// </summary>
        public string? Word { get; set; }

        /// <summary>
        /// Nguồn tra cứu từ
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Số lần tra cứu từ
        /// </summary>
        public int NumberOfSearch { get; set; } = 1;
    }
}
