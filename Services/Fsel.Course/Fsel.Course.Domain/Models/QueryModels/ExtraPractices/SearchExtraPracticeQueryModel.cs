// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ExtraPractices
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SearchExtraPracticeQueryModel : BaseQueryModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumExtraPracticeType? Type { get; set; }
        public bool? IsActive { get; set; }
    }
}
