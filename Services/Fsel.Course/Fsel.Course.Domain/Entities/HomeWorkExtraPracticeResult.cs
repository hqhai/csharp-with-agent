// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class HomeWorkExtraPracticeResult : BaseScoreResult, ISubmissionCount
    {
        private EnumSubmissionCount? _submissionCount;

        public EnumSubmissionCount? SubmissionCount
        {
            get
            {
                return _submissionCount.HasValue ? _submissionCount : EnumSubmissionCount.FirstSubmit;
            }
            set { _submissionCount = value; }
        }

        public HomeWork? HomeWork { get; set; }
        public Guid HomeWorkId { get; set; }
        public HomeWorkRetry? HomeWorkRetry { get; set; }
        public Guid HomeWorkRetryId { get; set; }

        public ICollection<HomeWorkExtraPracticeAnswer> HomeWorkExtraPracticeAnswers { get; set; } = new List<HomeWorkExtraPracticeAnswer>();
    }
}
