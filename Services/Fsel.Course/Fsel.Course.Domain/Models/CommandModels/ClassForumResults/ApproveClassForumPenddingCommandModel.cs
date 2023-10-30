// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ClassForumResults
{
    public class ApproveClassForumPenddingCommandModel
    {
        public Guid ClassForumResultId { get; set; }

        public bool IsApprove { get; set; }

        public long TaggetWordLimit { get; set; }

        public double TaggetTimeLimit { get; set; }
    }
}
