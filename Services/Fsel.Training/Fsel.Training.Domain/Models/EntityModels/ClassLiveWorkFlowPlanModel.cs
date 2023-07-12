// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ClassLiveWorkFlowPlanModel : BaseModel
    {
        public Guid LiveTimeFrameId { get; set; }

        public DateTime LiveDate { get; set; }

        public int VoteNumber { get; set; }
        public double Percent { get; set; }
        public bool IsActive { get; set; }

        public Guid ClassLiveWorkFlowId { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
    }
}
