// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class GameTopic : Entity
    {
        public EnumUnitNumber UnitOrder { get; set; }

        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Value { get; set; }

        public EnumCourseSkill Skill { get; set; }
        public Guid? SkillId { get; set; }
        public string? SkillName { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
        public ICollection<GameVocabulary> GameVocabularies { get; set; } = new List<GameVocabulary>();
    }
}
