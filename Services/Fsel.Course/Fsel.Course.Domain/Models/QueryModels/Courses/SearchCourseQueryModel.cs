// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.QueryModels.Courses
{
    public class SearchCourseQueryModel : BaseQueryModel
    {
        public Guid? TeacherId { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
        public EnumCourseType? CourseType { get; set; }

        public Guid? LevelId { get; set; }

        public Guid? ProgramId { get; set; }

        public IList<Guid>? LevelIds { get; set; }
        public IList<Guid>? ProgramIds { get; set; }
    }
}
