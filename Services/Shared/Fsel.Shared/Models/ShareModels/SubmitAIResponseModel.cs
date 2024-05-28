// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SubmitAIResponseModel
    {
        public string? GradingAlFeedback { get; set; }
        public Guid? ClassForumResultId { get; set; }
        public EnumSubmissionCount EnumSubmissionCount { get; set; }

    }

    public class SubmitMockTestResponseModel
    {
        public string? CriteriaName { get; set; }
        public int? DisplayOrder { get; set; }

        public Guid? MockTestResultId { get; set; }

        public string? GradingAlFeedBack { get; set; }
    }
}
