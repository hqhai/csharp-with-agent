// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class DictionarySearchHistoryModel : BaseModel
    {
        public string SearchTerm { get; set; } = string.Empty;

        public string? SearchContext { get; set; }

        public Guid? DictionaryAIId { get; set; }

        public string? SourceLanguage { get; set; }

        public string? TargetLanguage { get; set; }

        public bool FoundResult { get; set; }

        public int ResultCount { get; set; }

        public long ResponseTimeMs { get; set; }

        public DateTime SearchedAt { get; set; }
    }
}
