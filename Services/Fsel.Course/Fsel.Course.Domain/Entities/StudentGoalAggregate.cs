// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class StudentGoalAggregate : Entity
    {
        /// <summary>Tổng số bài đã hoàn thành (tích lũy toàn khóa).</summary>
        public int TotalCompletedLessons { get; set; }

        /// <summary>Tổng mục tiêu (toàn khóa) – ví dụ tổng số bài dự kiến cần học/hoàn thành.</summary>
        public int TotalTargetLessons { get; set; }

        public int ConsecutiveBehindWeeks { get; set; }
        public EnumCombinedProgress? CurrentCombinedProgress { get; set; }
        public EnumCombinedProgress? CombinedProgress { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? LevelId { get; set; }
        public EnumCourseType CourseType { get; set; }
        public bool IsActive { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SchoolName { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ClassName { get; set; }

        public Guid? ClassId { get; set; }
        public Guid? SchoolId { get; set; }
        public Guid StudentId { get; set; }
        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseGoalId { get; set; }
        public Guid CourseGoalConfigId { get; set; }
        public ICollection<StudentGoalSummary> StudentGoalSummaries { get; set; } = new List<StudentGoalSummary>();
    }
}
