// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TestScore : Entity
    {
        public EnumTestScoreCriteria? Criteria { get; set; }

        public string? Feedback { get; set; }

        public double Score { get; set; }

        public Guid? TestResultId { get; set; }
        public TestResult? TestResult { get; set; }
        public Guid? TestSectionResultId { get; set; }
        public TestSectionResult? TestSectionResult { get; set; }
        public Guid? TestSectionId { get; set; }
        public TestSection? TestSection { get; set; }
    }
}
