// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using Fsel.Course.Domain.Entities.FlowConfigs;

    public class TestResult : BaseLearnResult
    {
        public bool IsViewed { get; set; }
        public double? Score { get; set; }
        public int MaxHoursCompleted { get; set; }

        public Guid? TestId { get; set; }
        public Test? Test { get; set; }
        public Guid? TestGroupResultId { get; set; }
        public TestGroupResult? TestGroupResult { get; set; }
        public Guid? StepFlowId { get; set; }
        public StepFlow? StepFlow { get; set; }
        public Guid? ActionFlowId { get; set; }
        public ActionFlow? ActionFlow { get; set; }

        public DateTime? NewDate { get; set; }
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public ICollection<TestSectionResult> SectionResults { get; set; } = new List<TestSectionResult>();
        public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
        public ICollection<TestScore> TestScores { get; set; } = new List<TestScore>();
    }
}
