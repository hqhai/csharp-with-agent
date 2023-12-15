// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class QuestionTestModel
    {
        public Guid QuestionId { get; set; }
        public EnumSubAnswerStatus? Status { get; set; }
    }
}
