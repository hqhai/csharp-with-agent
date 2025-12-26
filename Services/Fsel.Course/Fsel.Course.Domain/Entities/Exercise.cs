// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Entities
{
    public class Exercise : Entity
    {
        /// <summary>
        /// Name Exercise
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Media Post
        /// </summary>
        public string? MediaPost { get; set; }

        /// <summary>
        /// Course Skill
        /// </summary>
        public EnumCourseSkill CourseSkill { get; set; }

        /// <summary>
        /// Media Post Ruby
        /// </summary>

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? MediaPostContentRuby { get; set; }

        public Skill? Skill { get; set; }
        public Guid? SkillId { get; set; }

        public ICollection<TimeCodeExercise> TimeCodeExercises { get; set; } = new List<TimeCodeExercise>();
        public ICollection<ExerciseQuestion> ExerciseQuestions { get; set; } = new List<ExerciseQuestion>();
        public ICollection<VideoTimeCodeAnswer> VideoTimeCodeAnswers { get; set; } = new List<VideoTimeCodeAnswer>();
        public ICollection<ExtraPracticeExercise> ExtraPracticeExercises { get; set; } = new List<ExtraPracticeExercise>();
    }
}
