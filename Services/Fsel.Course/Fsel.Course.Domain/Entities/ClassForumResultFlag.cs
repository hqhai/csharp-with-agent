// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class ClassForumResultFlag : Entity
    {
        public EnumClassForumResultFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public EnumClassForumResultFlagStatus Status { get; set; }

        public Guid ClassForumResultId { get; set; }

        public ClassForumResult? ClassForumResult { get; set; }
    }
}
