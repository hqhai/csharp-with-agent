// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class ExamPracticeScore : Entity
    {
        public EnumMockTestScoreCriteria Criteria { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeedBack { get; set; }

        public long Score { get; set; }
        public ExamPracticeSection? ExamPracticeSection { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
        public ExamPracticeResult? ExamPracticeResult { get; set; }
        public Guid ExamPracticeResultId { get; set; }
    }
}
