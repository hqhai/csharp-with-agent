// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IEntities;

    public class MockTestResult : BaseLearnResult, ITokenResult
    {
        public Guid? GradingTeacherId { get; set; }

        public Course? Course { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CourseId { get; set; }

        public Unit? Unit { get; set; }
        public Guid? UnitId { get; set; }
        public MockTest? MockTest { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid MockTestId { get; set; }

        public int? TokenDone { get; set; }
        public int? TokenHighestStreak { get; set; }
        public int? TokenSuperFire { get; set; }
        public int? TokenQuestionReward { get; set; }
        public DateTime? GradingStartDate { get; set; }
        public ICollection<MockTestAnswer> MockTestAnswers { get; set; } = new List<MockTestAnswer>();
        public ICollection<MockTestScore> MockTestScores { get; set; } = new List<MockTestScore>();
        public ICollection<SectionGroupResult> SectionGroupResults { get; set; } = new List<SectionGroupResult>();
    }
}
