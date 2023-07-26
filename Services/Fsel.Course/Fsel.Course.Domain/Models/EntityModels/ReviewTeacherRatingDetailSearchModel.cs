// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ReviewTeacherRatingDetailSearchModel
    {
        public string? FullName { get; set; }
        public PagingItemsModel<ReviewTeacherRatingDetailModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewTeacherRatingDetailModel>();
    }

    public class ReviewTeacherRatingDetailModel : BaseModel
    {
        public string? Code { get; set; }
        public string? ReviewArea { get; set; }
        public double Starts { get; set; }
        public string? Feedback { get; set; }
    }
}
