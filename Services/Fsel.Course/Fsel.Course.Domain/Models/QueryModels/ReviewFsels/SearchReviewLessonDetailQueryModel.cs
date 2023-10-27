// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReviewLessonDetailQueryModel : BaseQueryModel
    {
        public Guid LessonId { get; set; }
        public int? NumberOfStars { get; set; }
    }
}
