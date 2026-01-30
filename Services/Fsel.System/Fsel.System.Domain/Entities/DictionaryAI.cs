// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using global::System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// DictionaryAI entity for semantic dictionary search with pgvector
    /// </summary>
    public class DictionaryAI : Entity
    {
        /// <summary>
        /// Từ/cụm/câu được bôi đen (ngôn ngữ nguồn)
        /// </summary>
        public string? HighlightedItemSource { get; set; }

        /// <summary>
        /// Từ/cụm/câu dịch (ngôn ngữ đích)
        /// </summary>
        public string? HighlightedItemTarget { get; set; }

        /// <summary>
        /// Định nghĩa (ngôn ngữ nguồn)
        /// </summary>
        public string? DefinitionSource { get; set; }

        /// <summary>
        /// Định nghĩa (ngôn ngữ đích)
        /// </summary>
        public string? DefinitionTarget { get; set; }

        /// <summary>
        /// JSON payload theo FR3 specification
        /// </summary>
        public string? JsonPayload { get; set; }

        /// <summary>
        /// Array of semantic chunks dạng JSON
        /// </summary>
        public string? NotesJson { get; set; }

        /// <summary>
        /// Câu ví dụ (ngôn ngữ nguồn)
        /// </summary>
        public string? ExampleSentenceSource { get; set; }

        /// <summary>
        /// Câu ví dụ (ngôn ngữ đích)
        /// </summary>
        public string? ExampleSentenceTarget { get; set; }

        /// <summary>
        /// Ngôn ngữ nguồn (en, ja, ko, zh, vi, ...)
        /// </summary>
        public string? SourceLanguage { get; set; }

        /// <summary>
        /// Ngôn ngữ đích (vi, en, ...)
        /// </summary>
        public string? TargetLanguage { get; set; }

        /// <summary>
        /// Có audio từ API cũ không
        /// </summary>
        public bool HasAudioFromLegacy { get; set; }

        /// <summary>
        /// Tên model AI đã sinh (gpt-4o-mini, ...)
        /// </summary>
        public string? ModelName { get; set; }

        /// <summary>
        /// Độ dài input
        /// </summary>
        public int InputLength { get; set; }

        /// <summary>
        /// Embedding vector (1536 dimensions) - pgvector
        /// </summary>
        public float[]? Embedding { get; set; }

        /// <summary>
        /// Similarity score (computed during search, not stored in DB)
        /// </summary>
        [NotMapped]
        public double Similarity { get; set; }
    }
}
