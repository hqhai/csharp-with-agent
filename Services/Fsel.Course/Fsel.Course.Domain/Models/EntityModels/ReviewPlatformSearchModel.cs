// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ReviewPlatformSearchModel
    {
        public double Starts { get; set; }
        public PagingItemsModel<ReviewPlatformModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewPlatformModel>();
    }

    public class ReviewPlatformModel : BaseModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid StudentId { get; set; }
        public double Starts { get; set; }
        public IList<ReviewPlatformStudentModel>? StudentReviewDetails { get; set; }
    }

    public class ReviewPlatformStudentModel
    {
        public Guid Id { get; set; }
        public EnumReviewQuestionType ReviewQuestionType { get; set; }
        public int VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
