// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Enums;

    public class StudentReviewDetailModel : BaseModel
    {
        public EnumReviewQuestionType ReviewQuestionType { get; set; }
        public double VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
