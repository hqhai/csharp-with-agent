// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class RateClassForumResultCommandModel
    {
        public int? FeedBackStars { get; set; }
        public string? FeedBackNote { get; set; }
        public IList<EnumFeedBackPositive>? FeedBackPositives { get; set; }
        public IList<EnumFeedBackNegative>? FeedBackNegatives { get; set; }
        public Guid ObjectId { get; set; }

        public EnumStudentFeedBackType? Type { get; set; }
    }
}
