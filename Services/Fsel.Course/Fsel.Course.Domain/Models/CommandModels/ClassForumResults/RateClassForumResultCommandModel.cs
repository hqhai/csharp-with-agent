// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    public class RateClassForumResultCommandModel
    {
        public int? FeedBackStars { get; set; }
        public string? FeedBackNote { get; set; }
        public Guid ClassForumResultId { get; set; }
    }
}
