// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class SubQuestionModel
    {
        public Guid Id { get; set; }
        public EnumCorrectStatus Status { get; set; }
        public EnumQuestionType QuestionType { get; set; }
    }
}
