// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    using Fsel.Shared.Enums;

    public class RateClassForumResultCommandModel
    {
        public int? FeedBackStars { get; set; }
        public string? FeedBackNote { get; set; }
        public IList<EnumFeedBackPositive>? FeedBackPositives { get; set; }
        public IList<EnumFeedBackNegative>? FeedBackNegatives { get; set; }
        public Guid ClassForumResultId { get; set; }
    }
}
