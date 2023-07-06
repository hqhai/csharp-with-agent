// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ClassLiveWorkFlowPlanModel : BaseModel
    {
        public Guid LiveTimeFrameId { get; set; }

        public DayOfWeek LiveDate { get; set; }

        public int VoteNumber { get; set; }

        public bool IsActive { get; set; }

        public Guid ClassLiveWorkFlowId { get; set; }
    }
}
