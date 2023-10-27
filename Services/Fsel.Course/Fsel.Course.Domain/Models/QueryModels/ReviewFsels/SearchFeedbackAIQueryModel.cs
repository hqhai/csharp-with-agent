// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.ReviewFsels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchFeedbackAIQueryModel : BaseQueryModel
    {
        public int? NumberOfStars { get; set; }
    }
}
