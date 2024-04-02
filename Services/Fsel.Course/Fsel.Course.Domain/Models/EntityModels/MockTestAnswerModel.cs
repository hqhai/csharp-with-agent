// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class MockTestAnswerModel : BaseAnswerModel
    {
        public Guid? SectionQuestionId { get; set; }
        public Guid MockTestResultId { get; set; }

        public int? TimeCount { get; set; }
        public int? WordCount { get; set; }

        public string? GradingAlFeedback { get; set; }

        public string? TaskResponse { get; set; }

        public string? Conherence { get; set; }

        public string? LexicalResourse { get; set; }

        public string? GrammaticalRage { get; set; }
    }
}
