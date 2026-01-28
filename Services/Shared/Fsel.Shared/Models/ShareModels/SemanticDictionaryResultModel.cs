// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    /// <summary>
    /// Request model cho semantic dictionary search
    /// </summary>
    public class SemanticDictionaryRequestModel
    {
        public string? HighlightedItem { get; set; }
        public string? SentenceContext { get; set; }
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
    }

    /// <summary>
    /// Response model cho semantic dictionary (FR3 format)
    /// </summary>
    public class SemanticDictionaryResultModel
    {
        public string? Id { get; set; }
        public string? HighlightedItemSource { get; set; }
        public string? HighlightedItemTarget { get; set; }
        public string? DefinitionSource { get; set; }
        public string? DefinitionTarget { get; set; }
        public string? JsonPayload { get; set; }
        public List<SemanticNoteModel>? Notes { get; set; }
        public string? ExampleSentenceSource { get; set; }
        public string? ExampleSentenceTarget { get; set; }
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
        public bool HasAudioFromLegacy { get; set; }
        public double SimilarityScore { get; set; }
        public bool IsFromCache { get; set; }
    }

    /// <summary>
    /// Note/Semantic chunk model
    /// </summary>
    public class SemanticNoteModel
    {
        public string? SemanticChunk { get; set; }
        public string? TypeSource { get; set; }
        public string? TypeTarget { get; set; }
        public string? ExplanationSource { get; set; }
        public string? ExplanationTarget { get; set; }
    }

    /// <summary>
    /// Queue model for sending semantic dictionary result
    /// </summary>
    public class SemanticDictionaryQueueModel
    {
        public string? UserId { get; set; }
        public SemanticDictionaryResultModel? Result { get; set; }
    }
}
