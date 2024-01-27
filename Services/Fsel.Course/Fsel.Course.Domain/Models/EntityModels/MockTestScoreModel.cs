// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Enums;

    public class MockTestScoreModel
    {
        public string? FeedBack { get; set; }
        public long? Score { get; set; }
        public EnumClassForumScoreCriteria Criteria { get; set; }
    }
}
