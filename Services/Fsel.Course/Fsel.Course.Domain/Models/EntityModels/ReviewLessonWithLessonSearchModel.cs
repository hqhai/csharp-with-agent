// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ReviewLessonWithLessonSearchModel
    {
        public string? Code { get; set; }
        public double Starts { get; set; }
        public PagingItemsModel<ReviewLessonWithLessonModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewLessonWithLessonModel>();
    }

    public class ReviewLessonWithLessonModel : BaseModel
    {
        public Guid TeacherId { get; set; }
        public string? FullName { get; set; }
        public string? Name { get; set; }
        public double Starts { get; set; }
        public double TotalStart { get; set; }

        public int TotalResult { get; set; }
    }
}
