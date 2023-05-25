// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class ClassForumScore : Entity
    {
        public string? Feedback { get; set; }

        public Guid ClassForumResultId { get; set; }

        public long? Score { get; set; }

        public EnumClassForumCriteria Criteria { get; set; }

        public ClassForumResult? ClassForumResult { get; set; }
    }
}
