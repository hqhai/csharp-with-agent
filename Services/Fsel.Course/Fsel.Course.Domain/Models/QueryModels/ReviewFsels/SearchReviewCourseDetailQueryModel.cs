// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReviewCourseDetailQueryModel : BaseQueryModel
    {
        public Guid CourseId { get; set; }
        public bool? IsSortStarts { get; set; }
    }
}
