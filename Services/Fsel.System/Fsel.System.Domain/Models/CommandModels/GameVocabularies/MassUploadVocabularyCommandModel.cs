// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GameVocabularies
{
    using Fsel.Shared.Enums;
    using OfficeOpenXml.Attributes;

    public class MassUploadVocabularyCommandModel
    {
        [EpplusTableColumn(Header = "Code")]
        public string? Code { get; set; }

        [EpplusTableColumn(Header = "Key")]
        public string? Key { get; set; }

        [EpplusTableColumn(Header = "CEFR Level")]
        public string? CefrLevel { get; set; }

        [EpplusTableColumn(Header = "FSEL Course")]
        public string? CourseLevel { get; set; }

        [EpplusTableColumn(Header = "FSEL Course Unit")]
        public string? UnitOrder { get; set; }

        [EpplusTableColumn(Header = "Alternate Spelling")]
        public string? AlternateSpelling { get; set; }

        [EpplusTableColumn(Header = "US Equivalent")]
        public string? UsEquivalent { get; set; }

        [EpplusTableColumn(Header = "Word Category")]
        public string? WordCategory { get; set; }

        [EpplusTableColumn(Header = "Part Of Speech")]
        public string? PartSpeech { get; set; }

        [EpplusTableColumn(Header = "Definition")]
        public string? Definition { get; set; }

        [EpplusTableColumn(Header = "Hint")]
        public string? Hint { get; set; }

        [EpplusTableColumn(Header = "Example Sentence")]
        public string? ExampleSentence { get; set; }

        [EpplusTableColumn(Header = "Image")]
        public string? Image { get; set; }

        [EpplusTableColumn(Header = "Audio")]
        public string? Audio { get; set; }

        [EpplusTableColumn(Header = "Synonym")]
        public string? Synonym { get; set; }

        [EpplusTableColumn(Header = "Antonym")]
        public string? Antonym { get; set; }

        [EpplusTableColumn(Header = "Phonetic Transcription")]
        public string? PhoneticTranscription { get; set; }
    }
}
