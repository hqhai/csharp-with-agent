// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.GameVocabularies
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using global::System;

    public class UpdateGameVocabularyCommandModel : BaseCommandModel
    {
        public string? Code { get; set; }
        public string? Key { get; set; }
        public EnumGameCefrLevel CefrLevel { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
        public EnumUnitNumber UnitOrder { get; set; }
        public EnumPartSpeech? PartSpeech { get; set; }
        public Guid? WordCategoryId { get; set; }
        public IList<UpdateGameVocabularyTypeCommandModel>? GameVocabularyTypeModels { get; set;}
    }

    public class UpdateGameVocabularyTypeCommandModel
    {
        public Guid? Id { get; set; }
        public EnumGameVocabType GameVocabType { get; set; }
        public string? QuestionContent { get; set; }
    }
}
