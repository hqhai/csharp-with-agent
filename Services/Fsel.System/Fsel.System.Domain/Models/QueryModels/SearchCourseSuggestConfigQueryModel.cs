// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Enums;

    public class SearchCourseSuggestConfigQueryModel : BaseQueryModel
    {
        public int? FromAge { get; set; }

        public int? ToAge { get; set; }

        public EnumCourseLevel? PlacementTestLevel { get; set; }

        public EnumCourseSuggestType? Type { get; set; }
    }
}
