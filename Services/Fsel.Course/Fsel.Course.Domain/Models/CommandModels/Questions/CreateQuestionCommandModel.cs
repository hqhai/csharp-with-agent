// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Questions
{
    public class CreateQuestionCommandModel
    {
        public Guid? Id { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public string? Explanation { get; set; }
        public string? Description { get; set; }
        public bool Ungraded { get; set; }
        public object? Config { get; set; }
    }
}
