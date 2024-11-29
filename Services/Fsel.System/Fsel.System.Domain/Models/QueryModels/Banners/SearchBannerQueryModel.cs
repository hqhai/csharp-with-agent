// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels.Banners
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchBannerQueryModel : BaseQueryModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
