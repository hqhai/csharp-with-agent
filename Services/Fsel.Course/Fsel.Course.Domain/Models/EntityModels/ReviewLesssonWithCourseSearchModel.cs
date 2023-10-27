// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ReviewLessonWithCourseSearchModel
    {
        public double Stars { get; set; }
        public PagingItemsModel<ReviewLessonWithCourseModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewLessonWithCourseModel>();
    }

    public class ReviewLessonWithCourseModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public double Stars { get; set; }
    }
}
