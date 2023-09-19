// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchStudentFeedbackAIQueryModel : BaseQueryModel
    {
        public int? FeedBackStars { get; set; }
        public Guid? ClassForumId { get; set; }
    }
}
