// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResultFlags
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;

    public class RateClassForumResultFlagCommandModel : BaseCommandModel
    {
        public EnumClassForumResultFlagIssue FlagIssue { get; set; }

        public string? FeedBack { get; set; }

        public Guid ClassForumResultId { get; set; }
    }
}
