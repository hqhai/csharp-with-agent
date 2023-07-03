// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlows
{
    public class CreateClassLiveWorkFlowCommandModel
    {
        public Guid ClassLiveCalendarId { get; set; }
        public string? Description { get; set; }
        public IList<CreateClassWorkFlowCommandModel>? WorkFlows { get; set; }
    }

    public class CreateClassWorkFlowCommandModel
    {
        public Guid LiveTimeFrameId { get; set; }
        public DateTime LiveDate { get; set; }
    }
}
