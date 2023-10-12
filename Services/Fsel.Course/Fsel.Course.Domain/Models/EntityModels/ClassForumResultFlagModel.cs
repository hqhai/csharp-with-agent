// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class ClassForumResultFlagModel : BaseModel
    {
        public EnumClassForumResultFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public EnumClassForumResultFlagStatus Status { get; set; }

        public Guid ClassForumResultId { get; set; }
    }
}
