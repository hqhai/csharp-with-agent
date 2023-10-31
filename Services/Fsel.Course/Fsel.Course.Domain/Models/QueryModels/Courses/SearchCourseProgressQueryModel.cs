// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Courses
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchCourseProgressQueryModel : BaseQueryModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
