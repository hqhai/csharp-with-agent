// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestResults
{
    using Fsel.Course.Domain.Enums;

    public class CreateMockTestScoreCommandModel
    {
        public string? Feedback { get; set; }
        public long Score { get; set; }
        public Guid SectionGroupId { get; set; }
        public EnumClassForumScoreCriteria Criteria { get; set; }
    }
}
