// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class MockTestScore : Entity
    {
        public EnumClassForumScoreCriteria Criteria { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FeedBack { get; set; }

        public long Score { get; set; }
        public SectionGroup? SectionGroup { get; set; }
        public Guid SectionGroupId { get; set; }
        public MockTestResult? MockTestResult { get; set; }
        public Guid MockTestResultId { get; set; }
    }
}
