// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class StudentFeedback : Entity
    {
        public EnumStudentFeedBackType Type { get; set; }

        public EnumFeature Feature { get; set; }

        [Range(0, 5, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int? FeedBackStars { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeedBackNote { get; set; }

        public string? FeedBackPositivesStr { get; set; }

        [NotMapped]
        public IList<EnumFeedBackPositive>? FeedBackPositives
        {
            get { return ConvertHelper.Deserialize<IList<EnumFeedBackPositive>>(FeedBackPositivesStr); }
            set { FeedBackPositivesStr = ConvertHelper.Serialize(value); }
        }

        public string? FeedBackNegativesStr { get; set; }

        [NotMapped]
        public IList<EnumFeedBackNegative>? FeedBackNegatives
        {
            get { return ConvertHelper.Deserialize<IList<EnumFeedBackNegative>>(FeedBackNegativesStr); }
            set { FeedBackNegativesStr = ConvertHelper.Serialize(value); }
        }

        public Guid ObjectId { get; set; }
    }
}
