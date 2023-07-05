// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlowPlans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Training.Domain.Models.EntityModels;

    public class CreateClassLiveWorkFlowPlanCommandModel
    {
        public Guid LiveTimeFrameId { get; set; }

        public DayOfWeek LiveDate { get; set; }

        public int VoteNumber { get; set; }

        public bool IsActive { get; set; }

        /*   public Guid ClassLiveWorkFlowId { get; set; }*/
    }
}
