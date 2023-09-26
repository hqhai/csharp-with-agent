// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ReviewLessonDetailSearchModel
    {
        public string? Name { get; set; }
        public double Stars { get; set; }
        public PagingItemsModel<ReviewLessonDetailModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewLessonDetailModel>();
    }

    public class ReviewLessonDetailModel : BaseModel
    {
        public Guid StudentId { get; set; }
        public string? FullName { get; set; }
        public string? Code { get; set; }
        public string? ClassCode { get; set; }
        public double Stars { get; set; }
        public string? Feedback { get; set; }
    }
}
