// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels.Banners
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using global::System.Linq;
    using global::System.Text.Json.Serialization;

    public class SearchBannerQueryModel : BaseQueryModel
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public IList<string>? CourseLevels { get; set; }

        [JsonIgnore]
        public IList<EnumCourseLevel>? ListCourseLevels
        { get { return CourseLevels.ToList<EnumCourseLevel>(); } }
    }
}
