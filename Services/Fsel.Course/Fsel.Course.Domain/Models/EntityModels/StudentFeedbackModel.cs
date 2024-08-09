// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class StudentFeedbackModel : BaseModel
    {
        public EnumStudentFeedBackType Type { get; set; }

        public EnumFeature Feature { get; set; }

        public int? FeedBackStars { get; set; }

        public string? FeedBackNote { get; set; }

        public string? FeedBackPositivesStr { get; set; }

        public string? FeedBackNegativesStr { get; set; }
    }
}
