// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ReviewLessonWithUnitSearchModel
    {
        public string? Code { get; set; }
        public double Stars { get; set; }
        public PagingItemsModel<ReviewLessonWithUnitModel> PagingItemsModel { get; set; } = new PagingItemsModel<ReviewLessonWithUnitModel>();
    }

    public class ReviewLessonWithUnitModel : BaseModel
    {
        public string? Code { get; set; }
        public IList<Guid>? TeacherIds { get; set; }
        public IList<string>? TeacherNames { get; set; }
        public double Stars { get; set; }
    }
}
