// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.HomeWorks
{
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;

    public class SearchHomeWorkQueryModel : BaseQueyModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }

        public EnumCourseSkill? CourseSkill { get; set; }
    }
}
