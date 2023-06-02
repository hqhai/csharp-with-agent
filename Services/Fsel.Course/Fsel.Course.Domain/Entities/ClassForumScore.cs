// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class ClassForumScore : Entity
    {
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Feedback { get; set; }

        public Guid ClassForumResultId { get; set; }

        public long? Score { get; set; }

        public EnumClassForumScoreCriteria Criteria { get; set; }

        public ClassForumResult? ClassForumResult { get; set; }
    }
}
