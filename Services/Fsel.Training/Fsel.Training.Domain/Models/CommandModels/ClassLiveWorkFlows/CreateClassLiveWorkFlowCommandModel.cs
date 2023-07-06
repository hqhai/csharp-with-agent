// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlows
{
    using Fsel.Training.Domain.Enums;
    using Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlowPlans;

    public class CreateClassLiveWorkFlowCommandModel
    {
        public Guid ClassLiveCalendarId { get; set; }
        public string? Description { get; set; }
        public IList<CreateClassLiveWorkFlowPlanCommandModel>? ClassLiveWorkFlowPlans { get; set; }
        public EnumWorkFlowType Type { get; set; }
    }
}
