// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using Fsel.Core.Entities;

    public class ExamPracticeRetry : Entity
    {
        public int RetryCount { get; set; }
        public ExamPractice? ExamPractice { get; set; }
        public Guid ExamPracticeId { get; set; }
        public Guid StudentId { get; set; }
        public ICollection<ExamPracticeResult> ExamPracticeResults { get; set; } = new List<ExamPracticeResult>();
    }
}
