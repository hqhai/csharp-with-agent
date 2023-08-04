// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class MockTestScoreModel : BaseModel
    {
        public string? FeedBack { get; set; }
        public Guid MockTestResultId { get; set; }
        public long? Score { get; set; }
        public Guid SectionGroupId { get; set; }
        public EnumClassForumScoreCriteria Criteria { get; set; }
    }
}
