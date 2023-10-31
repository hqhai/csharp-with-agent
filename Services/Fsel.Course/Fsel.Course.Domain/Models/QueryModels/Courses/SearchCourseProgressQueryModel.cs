// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Courses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchCourseProgressQueryModel :  BaseQueryModel
    {
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
