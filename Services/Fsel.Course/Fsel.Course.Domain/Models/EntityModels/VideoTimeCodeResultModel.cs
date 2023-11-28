// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeResultModel : BaseResultScoreModel
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public double WorkingTime { get; set; }
        public double RetryWorkingTime { get; set; }
        public double RemainingTime { get; set; }
        public bool IsWorking { get; set; }
    }
}
