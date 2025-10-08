// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfigs
{
    using System;

    public class TestAnswer : BaseAnswer
    {
        public string? GradingAlFeedback { get; set; }

        public int? TimeCount { get; set; }

        public int? WordCount { get; set; }

        public string? SpeechTextAnswer { get; set; }

        public double? PronunciationScore { get; set; }

        public int RetryTime { get; set; }


        public TestResult? TestResult { get; set; }
        public Guid? TestResultId { get; set; }

        public TestSectionResult? TestSectionResult { get; set; }
        public Guid? TestSectionResultId { get; set; }

        public Guid? StudentId { get; set; }

        public Question? Question { get; set; }
        public Guid? QuestionId { get; set; }

        public TestSection? TestSection { get; set; }
        public Guid? TestSectionId { get; set; }
    }
}
