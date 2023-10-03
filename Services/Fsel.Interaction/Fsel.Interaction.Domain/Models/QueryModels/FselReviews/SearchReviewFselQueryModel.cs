// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.FselReviews
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchReviewFselQueryModel : BaseQueryModel
    {
        public Guid? CourseId { get; set; }
        public int? NumberOfStars { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
