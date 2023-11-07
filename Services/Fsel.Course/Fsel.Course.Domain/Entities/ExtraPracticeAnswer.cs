// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Shared.Enums;

    public class ExtraPracticeAnswer : BaseAnswer
    {
        public Question? Question { get; set; }
        public ExtraPracticeResult? ExtraPracticeResult { get; set; }
        public ExtraPracticeExerciseResult? ExtraPracticeExerciseResult { get; set; }
        public SectionTimeCode? SectionTimeCode { get; set; }
        public Section? Section { get; set; }
        public VideoTimeCode? VideoTimeCode { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
        public Guid? ExtraPracticeExerciseResultId { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? VideoTimeCodeId { get; set; }
        public Guid? SectionId { get; set; }
        public EnumAnswerStatus? Status { get; set; }
    }
}
