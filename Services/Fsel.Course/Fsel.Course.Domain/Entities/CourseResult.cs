// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class CourseResult : BaseScoreResult, IModuleLifeCycle
    {
        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public override double Percent { get; set; }

        [NotMapped]
        public override double PercentModule { get; set; }

        public Course? Course { get; set; }
        public Guid CourseId { get; set; }
        public EnumWorkingStatus WorkingStatus { get; set; }
        public DateTime? NewDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public ICollection<TestGroupResult> TestGroupResults { get; set; } = new List<TestGroupResult>();
        public ICollection<UnitResult> UnitResults { get; set; } = new List<UnitResult>();
        public ICollection<LessonResult> LessonResults { get; set; } = new List<LessonResult>();
        public ICollection<CourseChangingHistory> CourseChangingHistories { get; set; } = new List<CourseChangingHistory>();
    }
}
