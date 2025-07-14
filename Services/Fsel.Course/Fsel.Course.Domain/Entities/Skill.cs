// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Common.Attributes;

    public class Skill : Entity
    {
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FilePath { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[\x21-\x7E]+$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Name { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[\x21-\x7E]+$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? Code { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        public ICollection<SectionGroup> SectionGroups { get; set; } = new List<SectionGroup>();
        public ICollection<ClassForum> ClassForums { get; set; } = new List<ClassForum>();
        public ICollection<HomeWork> HomeWorks { get; set; } = new List<HomeWork>();
        public ICollection<LessonInstruction> LessonInstructions { get; set; } = new List<LessonInstruction>();
        public ICollection<ExtraPracticeExerciseResult> ExtraPracticeExerciseResults { get; set; } = new List<ExtraPracticeExerciseResult>();
        public ICollection<SkillLevel> SkillLevels { get; set; } = new List<SkillLevel>();
    }
}
