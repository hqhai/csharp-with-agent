// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestResults
{
    using Fsel.Shared.Enums;

    public class ReportFeedbackMockTestCommandModel
    {
        public int? FeedBackStars { get; set; }
        public string? FeedBackNote { get; set; }
        public IList<EnumFeedBackPositive>? FeedBackPositives { get; set; }
        public IList<EnumFeedBackNegative>? FeedBackNegatives { get; set; }
        public Guid ObjectId { get; set; }
    }
}
