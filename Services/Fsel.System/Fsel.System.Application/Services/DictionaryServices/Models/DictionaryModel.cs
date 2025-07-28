// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.DictionaryServices.Models
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Models.ShareModels;
    using global::System.Text.Json.Serialization;
    using Refit;

    public class DictionaryModel
    {
        [AliasAs("word")]
        [JsonPropertyName("word")]
        public string? Word { get; set; }

        [AliasAs("phonetic")]
        [JsonPropertyName("phonetic")]
        public string? Phonetic { get; set; }

        [AliasAs("phonetics")]
        [JsonPropertyName("phonetics")]
        public IList<PhoneticModel>? Phonetics { get; set; }

        [AliasAs("origin")]
        [JsonPropertyName("origin")]
        public string? Origin { get; set; }

        [AliasAs("meanings")]
        [JsonPropertyName("meanings")]
        public IList<MeaningsModel>? Meanings { get; set; }

        [AliasAs("license")]
        [JsonPropertyName("license")]
        public LicenseModel? License { get; set; }

        [AliasAs("sourceUrls")]
        [JsonPropertyName("sourceUrls")]
        public IList<string>? SourceUrls { get; set; }

    }

    public class PhoneticModel
    {
        [AliasAs("text")]
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [AliasAs("audio")]
        [JsonPropertyName("audio")]
        public string? Audio { get; set; }

        [AliasAs("license")]
        [JsonPropertyName("license")]
        public LicenseModel? License { get; set; }
    }

    public class MeaningsModel
    {
        [AliasAs("partOfSpeech")]
        [JsonPropertyName("partOfSpeech")]
        public string? PartOfSpeech { get; set; }

        [AliasAs("definitions")]
        [JsonPropertyName("definitions")]
        public IList<DefinitionsModel>? Definitions { get; set; }

        [AliasAs("synonyms")]
        [JsonPropertyName("synonyms")]
        public IList<string>? Synonyms { get; set; }

        [AliasAs("antonyms")]
        [JsonPropertyName("antonyms")]
        public IList<string>? Antonyms { get; set; }
    }

    public class DefinitionsModel
    {
        [AliasAs("definition")]
        [JsonPropertyName("definition")]
        public string? Definition { get; set; }

        [AliasAs("example")]
        [JsonPropertyName("example")]
        public string? Example { get; set; }
    }

    public class LicenseModel
    {
        [AliasAs("name")]
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [AliasAs("url")]
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
