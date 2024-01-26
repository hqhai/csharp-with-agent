// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities.BandScoresConfigs;

    public class SectionGroupResultReportModel : BaseLearnResultModel
    {
        public Guid SectionGroupId { get; set; }
        public Guid? MockTestResultId { get; set; }
        public BandScoresReport? BandScoresReport { get; set; }
        public double TargetBandScore { get; set; }
        public bool IsCheckScoreColor { get; set; }
    }
}
