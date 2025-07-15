// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.QueryModels.Courses
{
    public class SearchCourseQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }

        public EnumCourseLevel? CourseLevel { get; set; }

        public Guid? LevelId { get; set; }

        public Guid? ProgramId { get; set; }
    }
}
