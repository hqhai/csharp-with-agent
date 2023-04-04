// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class LessonInstruction : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Instruction { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
        public Lesson? Lesson { get; set; }
        public Guid LessonId { get; set; }
    }
}
