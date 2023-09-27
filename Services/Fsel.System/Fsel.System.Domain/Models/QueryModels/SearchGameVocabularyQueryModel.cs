// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchGameVocabularyQueryModel : BaseQueryModel
    {
        public EnumGameCourseLevel? CourseLevel { get; set; }
        public EnumUnitOrder? UnitOrder { get; set; }
        public Guid? WordCategoryId { get; set; }
        public EnumPartSpeech? PartSpeech { get; set; }
        public Guid? GameCenterId { get; set; }

    }
}
