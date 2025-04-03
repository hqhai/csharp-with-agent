// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors
{
    using Fsel.Shared.Enums;

    public class CreateQuestionExplanationErrorCommandModel
    {
        public string? Feedback { get; set; }
        public EnumFeedbackExplanation FeedbackExplanation { get; set; }
        public Guid QuestionId { get; set; }
        public EnumFeatureExplanationType ExplanationType { get; set; }
        public Guid ObjectResultId { get; set; }
    }
}
