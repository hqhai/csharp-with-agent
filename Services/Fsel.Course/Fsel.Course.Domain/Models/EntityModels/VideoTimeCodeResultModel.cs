// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class VideoTimeCodeResultModel : BaseLearnResultModel, ITokenResult
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public double RetryWorkingTime { get; set; }
        public double RemainingTime { get; set; }
        public int TokenDone { get; set; }
        public int TokenHighestStreak { get; set; }
        public int TokenSuperFire { get; set; }
        public bool IsWorking { get; set; }
    }
}
