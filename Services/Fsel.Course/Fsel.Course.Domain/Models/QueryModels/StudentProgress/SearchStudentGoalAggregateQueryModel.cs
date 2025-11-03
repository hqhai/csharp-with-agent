// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.StudentProgress
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class SearchStudentGoalAggregateQueryModel : BaseQueryModel
    {
        public EnumCourseType? CourseType { get; set; }
        public Guid? SchoolId { get; set; }
        public string? ClassIdStr { get; set; }
        public EnumCombinedProgress? CombinedProgress { get; set; }
    }
}
