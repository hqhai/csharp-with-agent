// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    public class SubmitAIResponseModel
    {
        public string? GradingAlFeedback { get; set; }
        public Guid? ClassForumDetailResultId { get; set; }
    }

    public class SubmitMockTestResponseModel
    {
        public string? CriteriaName { get; set; }
        public int? DisplayOrder { get; set; }

        public Guid? MockTestResultId { get; set; }

        public string? GradingAlFeedBack { get; set; }
    }
}