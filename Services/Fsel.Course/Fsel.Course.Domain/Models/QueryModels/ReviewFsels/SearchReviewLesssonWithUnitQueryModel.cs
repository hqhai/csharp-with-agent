// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchReviewLesssonWithUnitQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }
        public Guid CourseId { get; set; }
    }
}
