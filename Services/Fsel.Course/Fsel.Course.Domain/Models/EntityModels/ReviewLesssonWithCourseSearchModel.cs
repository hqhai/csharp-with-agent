// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class ReviewLesssonWithCourseSearchModel
    {
        public double Starts { get; set; }
        public PagingItemsModel<ReviewLesssonWithCourseModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewLesssonWithCourseModel>();
    }

    public class ReviewLesssonWithCourseModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public double Starts { get; set; }
    }
}
