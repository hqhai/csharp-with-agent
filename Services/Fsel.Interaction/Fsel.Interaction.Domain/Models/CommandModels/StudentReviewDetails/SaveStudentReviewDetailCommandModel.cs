// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.CommandModels.StudentReviewDetails
{
    using Fsel.Shared.Enums;

    public class SaveStudentReviewDetailCommandModel
    {
        public EnumReviewQuestionType QuestionType { get; set; }
        public double VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
