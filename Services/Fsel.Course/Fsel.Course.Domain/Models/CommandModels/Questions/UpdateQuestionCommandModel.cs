// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Questions
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UpdateQuestionCommandModel
    {
        public Guid? Id { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public string? Explanation { get; set; }
        public string? Description { get; set; }
        public bool Ungraded { get; set; }
        public object? Config { get; set; }
    }
}