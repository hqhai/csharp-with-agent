// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.CommandModels.Questions
{
    using Fsel.Shared.Enums;

    public class UpdateQuestionCommandModel
    {
        public Guid? Id { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public bool Ungraded { get; set; }
        public string? Explanation { get; set; }
        public int CorrectTotal { get; set; }
        public object? Config { get; set; }
        public string? Description { get; set; }
    }
}
