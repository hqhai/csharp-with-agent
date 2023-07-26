// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchReviewCourseQueryModel : BaseQueryModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }   
    }
}
