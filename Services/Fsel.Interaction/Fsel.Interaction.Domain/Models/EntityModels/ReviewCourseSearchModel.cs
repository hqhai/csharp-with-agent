// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class CourseReviewSearchModel
    {
        public double Stars { get; set; }
        public PagingItemsModel<CourseReviewModel> PagingItemsModel { get; set; } = new PagingItemsModel<CourseReviewModel>();
    }

    public class CourseReviewModel : BaseModel
    {
        public string? Code { get; set; }
        public Guid? CourseId { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public double Stars { get; set; }
    }
}
