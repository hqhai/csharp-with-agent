// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.LiveTimeFrames
{
    public class SaveLiveTimeFrameCommandModel
    {
        public Guid? Id { get; set; }
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
