// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.HomeWorkConfigs
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchHomeWorkConfigQueryModel : BaseQueryModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseSkill? CourseSkill { get; set; }
        public EnumCourseType? CourseType { get; set; }
        public Guid? CreatedUserId { get; set; }
    }
}
