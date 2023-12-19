// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.QuestionForms
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class CreateQuestionFormCommandModel
    {
        public string? Name { get; set; }
        public EnumQuestionType Type { get; set; }
        public object? Config { get; set; }
    }
}
