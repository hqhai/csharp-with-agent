// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlowPlans
{
    using System;

    public class CreateClassLiveWorkFlowPlanCommandModel
    {
        public Guid LiveTimeFrameId { get; set; }

        public DayOfWeek LiveDate { get; set; }
    }
}
