// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReviewLesssonWithLessonQueryModel : BaseQueryModel
    {
        public Guid UnitId { get; set; }
        public Guid? TeacherId { get; set; }
    }
}
