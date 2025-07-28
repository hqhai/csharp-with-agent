// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class DictionaryQueueModel
    {
        public IList<DictionaryQueueItemModel>? Dictionary { get; set; }
    }

    public class DictionaryQueueItemModel
    {
        public string? Word { get; set; }

        public string? Type { get; set; }

        public PhoneticModel? Phonetic { get; set; }

        public IList<string>? Synonyms { get; set; }

        public IList<string>? Antonyms { get; set; }

        public IList<PartOfSpeechModel>? PartOfSpeechs { get; set; }
    }

    public class PartOfSpeechModel
    {
        public string? PartOfSpeech { get; set; }

        public IList<DefinitionsModel>? Definitions { get; set; }
    }

    public class PhoneticModel
    {
        public string? Text { get; set; }

        public string? Audio { get; set; }
    }

    public class DefinitionsModel
    {
        public string? Definition { get; set; }

        public IList<Example>? Examples { get; set; }
    }

    public class Example
    {
        public string? ExampleVn { get; set; }

        public string? ExampleEn { get; set; }
    }
}
