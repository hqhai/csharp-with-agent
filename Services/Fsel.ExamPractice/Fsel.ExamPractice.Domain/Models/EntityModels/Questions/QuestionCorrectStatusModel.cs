// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.Questions
{
    using System;
    using Fsel.Shared.Enums;

    public class QuestionCorrectStatusModel
    {
        public Guid QuestionId { get; set; }
        public EnumCorrectStatus? Status { get; set; }
    }
}
