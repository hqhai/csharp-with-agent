// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class ExtraPracticeAnswerModel
    {
        public object? Answer { get; set; }
        public int CorrectCount { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
        public EnumTimeCodeStatus? Status { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
    }
}
