// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchCourseGoalQueryModel : BaseQueryModel
    {
        public string? CourseLevelStr { get; set; }

        public string? CourseTypeStr { get; set; }

        public string? SchoolIdStr { get; set; }

        public string? ClassIdStr { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseType? CourseType { get; set; }
    }
}
