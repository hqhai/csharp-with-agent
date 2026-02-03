// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.DictionaryServices.Models
{
    using global::System.Text.Json.Serialization;

    public class NoteModel
    {
        [JsonPropertyName("semanticChunk")]
        public string? SemanticChunk { get; set; }

        [JsonPropertyName("typeSource")]
        public string? TypeSource { get; set; }

        [JsonPropertyName("typeTarget")]
        public string? TypeTarget { get; set; }

        [JsonPropertyName("explanationSource")]
        public string? ExplanationSource { get; set; }

        [JsonPropertyName("explanationTarget")]
        public string? ExplanationTarget { get; set; }
    }

    /// <summary>
    /// AI response model (FR3 format)
    /// </summary>
    public class AIResponseModel
    {
        [JsonPropertyName("highlighted_item_source")]
        public string? HighlightedItemSource { get; set; }

        [JsonPropertyName("highlighted_item_target")]
        public string? HighlightedItemTarget { get; set; }

        [JsonPropertyName("definition_source")]
        public string? DefinitionSource { get; set; }

        [JsonPropertyName("definition_target")]
        public string? DefinitionTarget { get; set; }

        [JsonPropertyName("notes")]
        public List<NoteModel>? Notes { get; set; }

        [JsonPropertyName("example_sentence_source")]
        public string? ExampleSentenceSource { get; set; }

        [JsonPropertyName("example_sentence_target")]
        public string? ExampleSentenceTarget { get; set; }

        /// <summary>
        /// Model name used for AI generation
        /// </summary>
        [JsonPropertyName("modelName")]
        public string? ModelName { get; set; }
    }

}
