// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestResults
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class CreateMockTestScoreCommandModel
    {
        public string? FeedBack { get; set; }
        public long Score { get; set; }
        public Guid SectionGroupId { get; set; }
        public EnumMockTestScoreCriteria Criteria { get; set; }
    }
}
