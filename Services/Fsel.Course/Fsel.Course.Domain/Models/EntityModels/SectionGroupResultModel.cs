// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.BandScoresConfigs;
    using Fsel.Course.Domain.IEntities;

    public class SectionGroupResultModel : BaseLearnResultModel, ITokenResult
    {
        public Guid SectionGroupId { get; set; }
        public Guid? MockTestResultId { get; set; }
        public Guid? FinalTestResultId { get; set; }
        public Guid? ExtraPracticeResultId { get; set; }
        public Guid? PlacementTestResultId { get; set; }
        public Guid? CurrentSectionTimeCodeId { get; set; }
        public bool? IsTestDone { get; set; }
        public double RemainingTime { get; set; }
        public BandScoresReport? BandScoresReport { get; set; }
        public double TargetBandScore { get; set; }
        public bool IsCheckScoreColor { get; set; }
        public bool IsFeedBack { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
    }
}
