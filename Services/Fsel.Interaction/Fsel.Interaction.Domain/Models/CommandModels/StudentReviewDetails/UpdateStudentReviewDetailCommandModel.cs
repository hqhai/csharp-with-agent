// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.StudentReviewDetails
{
    using Fsel.Interaction.Domain.Enums;

    public class UpdateStudentReviewDetailCommandModel
    {
        public Guid Id { get; set; }
        public EnumReviewQuestionType QuestionType { get; set; }
        public double VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
