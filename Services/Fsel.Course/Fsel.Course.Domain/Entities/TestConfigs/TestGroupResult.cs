// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;

    public class TestGroupResult : Entity
    {
        public double Percent { get; set; }

        public EnumTestType TestType { get; set; }

        public DateTime? CompletionDate { get; set; }
        public EnumResultStatus Status { get; set; }

        public Guid? StudentId { get; set; }

        public Guid? CourseId { get; set; }
        public Course? Course { get; set; }
        public Guid? UnitId { get; set; }
        public Unit? Unit { get; set; }
        public Guid? CourseResultId { get; set; }
        public CourseResult? CourseResult { get; set; }
        public Guid? UnitResultId { get; set; }
        public UnitResult? UnitResult { get; set; }
        public Guid? CourseModuleId { get; set; }
        public CourseModule? CourseModule { get; set; }
        public Guid? UnitModuleId { get; set; }
        public UnitModule? UnitModule { get; set; }

        public Guid? LevelId { get; set; }
        public Level? Level { get; set; }

        /// <summary>
        /// Level hệ thống đề xuất
        /// </summary>
        public Guid? CurrentLevelId { get; set; }

        public Level? CurrentLevel { get; set; }

        /// <summary>
        /// Level được sử dụng để gửi email
        /// </summary>
        public Guid? EmailLevelId { get; set; }

        public Level? EmailLevel { get; set; }

        public Guid? FlowId { get; set; }
        public Flow? Flow { get; set; }

        public Guid? ProgramId { get; set; }

        public Category? Category { get; set; }

        public Guid? ProgramIdOfPt { get; set; }

        public ICollection<CourseChangingHistory> CourseChangingHistories { get; set; } = new List<CourseChangingHistory>();

        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
