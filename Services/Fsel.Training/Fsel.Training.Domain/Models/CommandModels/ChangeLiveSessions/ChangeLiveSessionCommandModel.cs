// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.CancelScheduleLives
{
    public class CancelScheduleLiveCommandModel
    {
        public Guid Id { get; set; }
        public IList<ChangeClassWordFlowPlanCommandModel>? ClassWordFlowPlans { get; set; }
    }

    public class ChangeClassWordFlowPlanCommandModel
    {
        public Guid ClassWordFlowPlanId { get;set; }
        public bool IsActive { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public DateTime LiveDate { get; set; }
    }
}
