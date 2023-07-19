// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPracticeAnswers
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.Enums;

    public class CreateExtraPracticeAnswerCommandModel
    {
        public EnumExtraPracticeType Type { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
        public Guid? ExtraPracticeExerciseId { get; set; }
        public IList<ExtraPracticeAnswerQuestionModel>? Answers { get; set; }
        public bool IsActive { get; set; }
    }

    public class ExtraPracticeAnswerQuestionModel
    {
        public Guid? SectionId { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public Guid? QuestionId { get; set; }
        public object? Answer { get; set; }
    }
}
