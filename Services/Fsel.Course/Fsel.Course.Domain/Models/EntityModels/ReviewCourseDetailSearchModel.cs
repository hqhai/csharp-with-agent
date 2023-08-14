// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ReviewCourseDetailSearchModel
    {
        public double Starts { get; set; }
        public string? Code { get; set; }
        public PagingItemsModel<ReviewCourseDetailModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewCourseDetailModel>();
    }

    public class ReviewCourseDetailModel : BaseModel
    {
        public EnumReviewType ReviewType { get; set; }
        public Guid StudentId { get; set; }
        public Guid? CourseId { get; set; }
        public string? Code { get; set; }
        public string? CodeStudent { get; set; }
        public string? ClassCode { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public double Starts { get; set; }
        public IList<ReviewCourseDetailInfoModel>? StudentReviewDetails { get; set; }
    }

    public class ReviewCourseDetailInfoModel
    {
        public Guid Id { get; set; }
        public EnumReviewQuestionType ReviewQuestionType { get; set; }
        public int VoteStars { get; set; }
        public string? Content { get; set; }
    }
}
