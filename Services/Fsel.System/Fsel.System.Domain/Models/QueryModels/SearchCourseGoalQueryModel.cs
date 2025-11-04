// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchCourseGoalQueryModel : BaseQueryModel
    {
        public string? CourseLevelStr { get; set; }

        public string? CourseTypeStr { get; set; }

        public string? SchoolIdStr { get; set; }

        public string? ClassIdStr { get; set; }
    }
}
