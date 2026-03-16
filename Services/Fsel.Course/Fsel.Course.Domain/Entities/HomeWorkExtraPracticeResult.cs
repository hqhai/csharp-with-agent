// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class HomeWorkExtraPracticeResult : BaseScoreResult, ISubmissionCount, IHighestStreak
    {
        public EnumSubmissionCount? SubmissionCount { get; set; }
        public int? HighestStreak { get; set; }
        public EnumWorkingStatus WorkingStatus { get; set; }
        public HomeWork? HomeWork { get; set; }
        public Guid HomeWorkId { get; set; }
        public HomeWorkRetry? HomeWorkRetry { get; set; }
        public Guid HomeWorkRetryId { get; set; }
        public ICollection<HomeWorkExtraPracticeAnswer> HomeWorkExtraPracticeAnswers { get; set; } = new List<HomeWorkExtraPracticeAnswer>();
    }
}
