// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReviewTeacherRatingDetailQueryModel : BaseQueryModel
    {
        public Guid TeacherId { get; set; }
        public bool IsSortStarts { get; set; }
    }
}
