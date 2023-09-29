// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class GameVocabularyModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Key { get; set; }
        public EnumGameCefrLevel CefrLevel { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
        public EnumUnitNumber UnitOrder { get; set; }
        public string? AlternateSpellingStr { get; set; }
        public IList<string>? AlternateSpelling { get; set; }
        public string? UsEquivalent { get; set; }
        public EnumPartSpeech? PartSpeech { get; set; }
        public string? Definition { get; set; }
        public string? Hint { get; set; }
        public string? ExampleSentence { get; set; }
        public string? ImagePath { get; set; }
        public string? AudioPath { get; set; }
        public string? Synonym { get; set; }
        public string? Antonym { get; set; }
        public string? PhoneticTranscription { get; set; }
        public Guid? GameCenterId { get; set; }
        public Guid? WordCategoryId { get; set; }
        public string? NameOfGame { get; set; }
        public string? WordCategory { get; set; }
    }
}
