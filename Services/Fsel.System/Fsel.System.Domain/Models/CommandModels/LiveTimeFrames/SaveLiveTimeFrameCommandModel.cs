// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.LiveTimeFrames
{
    public class SaveLiveTimeFrameCommandModel
    {
        public Guid? Id { get; set; }
        public double? StartTime { get; set; }

        public double? EndTime { get; set; }
    }
}
