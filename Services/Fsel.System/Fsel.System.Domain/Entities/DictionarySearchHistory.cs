// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;

    /// <summary>
    /// Entity lưu lịch sử tìm kiếm từ điển của user
    /// </summary>
    public class DictionarySearchHistory : Entity
    {
        /// <summary>
        /// ID của user thực hiện tìm kiếm
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Từ khóa tìm kiếm
        /// </summary>
        public string SearchTerm { get; set; } = string.Empty;

        /// <summary>
        /// Ngữ cảnh tìm kiếm (câu/văn bản chứa từ được highlight)
        /// </summary>
        public string? SearchContext { get; set; }

        /// <summary>
        /// ID của DictionaryAI được tìm thấy (nullable nếu không tìm thấy)
        /// </summary>
        public Guid? DictionaryAIId { get; set; }

        /// <summary>
        /// Ngôn ngữ nguồn
        /// </summary>
        public string? SourceLanguage { get; set; }

        /// <summary>
        /// Ngôn ngữ đích
        /// </summary>
        public string? TargetLanguage { get; set; }

        /// <summary>
        /// Có tìm thấy kết quả không
        /// </summary>
        public bool FoundResult { get; set; }

        /// <summary>
        /// Số kết quả trả về
        /// </summary>
        public int ResultCount { get; set; }

        /// <summary>
        /// Thời gian phản hồi (milliseconds)
        /// </summary>
        public long ResponseTimeMs { get; set; }
    }
}
