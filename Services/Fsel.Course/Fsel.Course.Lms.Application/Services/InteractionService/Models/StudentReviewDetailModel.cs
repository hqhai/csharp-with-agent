// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.InteractionService.Models
{
    using Fsel.Shared.Enums;

    public class StudentReviewDetailModel
    {
        public Guid Id { get; set; }
        public EnumReviewQuestionType ReviewQuestionType { get; set; }
        public int VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
