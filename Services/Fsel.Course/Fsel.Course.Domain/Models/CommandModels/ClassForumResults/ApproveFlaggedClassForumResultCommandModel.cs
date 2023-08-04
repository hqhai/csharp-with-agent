// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    using System;

    public class ApproveFlaggedClassForumResultCommandModel
    {
        public Guid ClassForumResultId { get; set; }

        public bool IsApprove { get; set; }
    }
}
