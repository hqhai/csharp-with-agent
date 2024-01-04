// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class QuestionCorrectStatusModel
    {
        public Guid QuestionId { get; set; }
        public EnumCorrectStatus? Status { get; set; }
    }
}
