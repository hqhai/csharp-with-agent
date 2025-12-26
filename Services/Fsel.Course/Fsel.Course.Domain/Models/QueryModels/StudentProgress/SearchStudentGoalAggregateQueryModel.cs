// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.StudentProgress
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchStudentGoalAggregateQueryModel : BaseQueryModel
    {
        public EnumCourseType? CourseType { get; set; }
        public Guid? SchoolId { get; set; }
        public string? ClassIdStr { get; set; }
        public EnumCombinedProgress? CombinedProgress { get; set; }
        public IList<EnumStatusStudentCampus>? StatusStudentCampus { get; set; }
        public string? ClassCampusCode { get; set; }
        public string? StudentCampusCode { get; set; }
        public string? SortDir { get; set; }
        public string? SortCompletedLessons { get; set; }
        public string? SortCompletedConfig { get; set; }
        public string? SortSlowProgress { get; set; }
    }
}
