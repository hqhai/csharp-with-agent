// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class StudentFeedbackModel
    {
        public EnumStudentFeedBackType Type { get; set; }

        public EnumFeature Feature { get; set; }

        public int? FeedBackStars { get; set; }

        public string? FeedBackNote { get; set; }

        public string? FeedBackPositivesStr { get; set; }

        public string? FeedBackNegativesStr { get; set; }
    }
}
