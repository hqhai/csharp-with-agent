// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ApproveFlaggedClassForumResultCommandModel
    {
        public Guid ClassForumResultId { get; set; }

        public bool IsApprove { get; set; }
    }
}
