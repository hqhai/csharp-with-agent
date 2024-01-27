// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class MockTestScoreModel
    {
        public string? FeedBack { get; set; }
        public long? Score { get; set; }
        public EnumMockTestScoreCriteria Criteria { get; set; }
    }
}
