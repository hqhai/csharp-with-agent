// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class VideoTimeCodeResultModel : BaseLearnResultModel
    {
        public Guid VideoResultId { get; set; }
        public Guid? CurrentVideoTimeCodeId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public double RetryWorkingTime { get; set; }
        public double RemainingTime { get; set; }
        public bool IsWorking { get; set; }
        public int? CorrectCountUngraded { get; set; }
        public int? CorrectTotalUngraded { get; set; }
    }
}
