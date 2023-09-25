// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseReviewDetailsModel
    {
        public double Starts { get; set; }
        public PagingItemsModel<ReviewCourseModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewCourseModel>();
    }

    public class ReviewCourseModel : BaseModel
    {
        public string? Code { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public double Starts { get; set; }
    }
}
