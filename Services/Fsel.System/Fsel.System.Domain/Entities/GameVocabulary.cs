// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class GameVocabulary : Entity
    {
        [Required]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Key { get; set; }
        public EnumGameCefrLevel CefrLevel { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
        public EnumUnitNumber UnitOrder { get; set; }
        public EnumPartSpeech? PartSpeech { get; set; }
        public Guid? PlatformId { get; set; }
        public Guid? WordCategoryId { get; set; }
        public GameTopic? GameTopic { get; set; }
        public ICollection<GameVocabularyType> GameVocabularyTypes { get; set; } = new List<GameVocabularyType>();
    }
}
