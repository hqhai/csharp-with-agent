// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class StudentReviewDetailModel
    {
        public Guid Id { get; set; }
        public EnumReviewQuestionType ReviewQuestionType { get; set; }
        public double VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
