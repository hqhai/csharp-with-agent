// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class StudentGoalSummary : Entity
    {
        /// <summary>Tổng số bài đã hoàn thành (tích lũy toàn khóa).</summary>
        public int TotalCompletedLessons { get; set; }

        /// <summary>Tổng mục tiêu (toàn khóa) – ví dụ tổng số bài dự kiến cần học/hoàn thành.</summary>
        public int TotalTargetLessons { get; set; }

        // Mục tiêu tuần cấu hình
        public int LessonsPerWeek { get; set; }

        public int CompletedLessons { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Thời điểm hoàn thành gần nhất</summary>
        public DateTime? LastCompletedAt { get; set; }

        public EnumProgressStatus? ProgressStatus { get; set; }
        public Guid StudentGoalAggregateId { get; set; }
        public StudentGoalAggregate? StudentGoalAggregate { get; set; }
    }
}
