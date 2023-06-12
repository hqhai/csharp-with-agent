// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ClassForumResults
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchClassForumResultAdminQueryModel : BaseQueryModel
    {
        public EnumCourseType? CourseType { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
