// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.Dictionaries
{
    using global::System.Text.Json.Serialization;

    public class ImportDictionaryCommandModel
    {
        [JsonPropertyName("word")]
        public string? Word { get; set; }

        [JsonPropertyName("partOfSpeech")]
        public string? PartOfSpeech { get; set; }

        [JsonPropertyName("phonetic")]
        public string? Phonetic { get; set; }

        [JsonPropertyName("meaning")]
        public string? Meaning { get; set; }

        [JsonPropertyName("examples")]
        public IList<Example>? Examples { get; set; }
    }

    public class Example
    {
        [JsonPropertyName("exampleVn")]
        public string? ExampleVn { get; set; }

        [JsonPropertyName("exampleEn")]
        public string? ExampleEn { get; set; }
    }
}
