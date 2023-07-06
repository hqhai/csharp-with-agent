// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class ClassLiveWorkFlowPlan : Entity
    {
        public Guid LiveTimeFrameId { get; set; }

        public DateTime LiveDate { get; set; }

        public int VoteNumber { get; set; }

        public bool IsActive { get; set; }

        public Guid ClassLiveWorkFlowId { get; set; }

        public ClassLiveWorkFlow? ClassLiveWorkFlow { get; set; }
    }
}
