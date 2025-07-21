// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.Configs
{
    using Fsel.ExamPractice.Domain.Enums;

    public class ExerciseConfig
    {
        public IList<Guid>? ExamPracticeSectionIds { get; set; }
        public bool IsAllPart { get; set; }
        public EnumPracticeTimeLimitOption? PracticeTimeLimitOption { get; set; }
        public int? ExecutionTime { get; set; }
    }
}
