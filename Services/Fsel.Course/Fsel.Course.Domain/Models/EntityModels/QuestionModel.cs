// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class QuestionModel
    {
        public Guid Id { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public int CorrectTotal { get; set; }
        public bool Ungraded { get; set; }
        public string? Explanation { get; set; }
        public object? Config { get; set; }
        public object? ResultAnswer { get; set; }
    }
}
