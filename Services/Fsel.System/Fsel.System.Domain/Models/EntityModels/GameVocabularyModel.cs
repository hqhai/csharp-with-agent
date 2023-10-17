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
        public EnumPartSpeech? PartSpeech { get; set; }
        public Guid? PlatformId { get; set; }
        public Guid? WordCategoryId { get; set; }
        public string? PlatformName { get; set; }
        public string? WordCategory { get; set; }
        public IList<GameVocabularyTypeModel>? GameVocabularyTypes { get; set; }
    }

    public class GameVocabularyTypeModel : BaseModel
    {
        public EnumGameVocabType GameVocabType { get; set; }
        public string? QuestionContent { get; set; }
        public IList<string>? AlternateSpelling { get; set; }
    }
}
