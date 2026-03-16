// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    public class TestSectionResult : BaseLearnResult
    {
        public override double PercentModule { get; set; }
        public double? ScoreModule { get; set; }
        public Guid? TestResultId { get; set; }

        public TestResult? TestResult { get; set; }

        public Guid? TestSectionId { get; set; }

        public TestSection? TestSection { get; set; }

        public Guid? ParentTestSectionResultId { get; set; }

        public Guid? CurrentSectionTimeCodeId { get; set; }
        public TestSectionResult? ParentTestSectionResult { get; set; }
        public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
        public ICollection<TestSectionResult> SectionResults { get; set; } = new List<TestSectionResult>();
        public ICollection<TestScore> TestScores { get; set; } = new List<TestScore>();
    }
}
