// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestResults
{
    public class ReportFeedbackMockTestCommandModel
    {
        public Guid MockTestResultId { get; set; }
        public int FeedbackStars { get; set; }
        public string? FeedbackNote { get; set; }
    }
}
