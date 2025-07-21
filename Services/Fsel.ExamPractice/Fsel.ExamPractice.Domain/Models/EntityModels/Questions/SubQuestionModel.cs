// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.Questions
{
    using Fsel.Shared.Enums;

    public class SubQuestionModel
    {
        public Guid Id { get; set; }
        public int IndexSubQuestion { get; set; }
        public Guid QuestionId { get; set; }
        public EnumCorrectStatus Status { get; set; }
        public EnumQuestionType QuestionType { get; set; }
    }
}
