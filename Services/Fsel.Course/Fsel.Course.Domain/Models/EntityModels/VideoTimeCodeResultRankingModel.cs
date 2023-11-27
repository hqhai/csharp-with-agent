// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Course.Domain.Enums;

    public class VideoTimeCodeResultRankingModel : BaseResultScoreModel
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public double RemainingTime { get; set; }
        public bool IsWorking { get; set; }

        public bool IsCurrentStudent { get; set; }

        public EnumTimeCodeType? TimeCodeType { get; set; }

        public double? TimeSpend { get; set; }
    }
}
