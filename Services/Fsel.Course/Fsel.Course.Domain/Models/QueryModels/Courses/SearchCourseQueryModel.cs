// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.QueryModels.Courses
{
    public class SearchCourseQueryModel : BaseQueyModel
    {
        public Guid? TeacherId { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
