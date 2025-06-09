// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class LessonInstruction : Entity
    {
        /// <summary>
        /// Chỉ dẫn
        /// </summary>

        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Instruction { get; set; }

        /// <summary>
        /// Skill
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        public Skill? Skill { get; set; }
        public Guid? SkillId { get; set; }
        public Lesson? Lesson { get; set; }
        public Guid LessonId { get; set; }
    }
}
