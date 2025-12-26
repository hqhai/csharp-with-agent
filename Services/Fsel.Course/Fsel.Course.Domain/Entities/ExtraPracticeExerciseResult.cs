// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums;

    public class ExtraPracticeExerciseResult : BaseResult
    {
        [NotMapped]
        public override double PercentModule { get; set; }

        /// <summary>
        /// Số luot lam bai của Student
        /// </summary>
        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int ExecuteCount { get; set; }

        public EnumCourseSkill CourseSkill { get; set; }
        public Skill? Skill { get; set; }
        public Guid? SkillId { get; set; }
        public ExtraPracticeResult? ExtraPracticeResult { get; set; }
        public Guid ExtraPracticeResultId { get; set; }
        public ExtraPracticeExercise? ExtraPracticeExercise { get; set; }
        public Guid ExtraPracticeExerciseId { get; set; }
        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
    }
}
