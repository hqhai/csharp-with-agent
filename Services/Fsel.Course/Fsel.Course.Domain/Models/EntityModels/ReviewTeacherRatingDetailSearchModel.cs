// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ReviewTeacherRatingDetailSearchModel
    {
        public string? FullName { get; set; }
        public PagingItemsModel<ReviewTeacherRatingDetailModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewTeacherRatingDetailModel>();
    }

    public class ReviewTeacherRatingDetailModel : BaseModel
    {
        public string? Code { get; set; }
        public string? ReviewArea { get; set; }
        public double Stars { get; set; }
        public string? Feedback { get; set; }
        public IList<EnumFeedBackPositive>? FeedbackPositive { get; set; }
        public IList<EnumFeedBackNegative>? FeedbackNegative { get; set; }
    }
}
