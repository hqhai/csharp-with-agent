// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GameVocabularies
{
    using Fsel.Shared.Enums;

    public class CreateGameVocabularyCommandModel
    {
        public string? Code { get; set; }
        public string? Key { get; set; }
        public EnumGameCefrLevel CefrLevel { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
        public EnumUnitNumber UnitOrder { get; set; }
        public EnumPartSpeech? PartSpeech { get; set; }
        public Guid? WordCategoryId { get; set; }
        public IList<CreateGameVocabularyTypeCommandModel>? GameVocabularyTypes { get; set; }
    }

    public class CreateGameVocabularyTypeCommandModel
    {
        public EnumGameVocabType GameVocabType { get; set; }
        public string? QuestionContent { get; set; }
    }
}
