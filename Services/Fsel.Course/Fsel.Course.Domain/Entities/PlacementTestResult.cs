// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Shared.Enums;

    public class PlacementTestResult : BaseScoreResult
    {
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }
        public EnumPlacementTestLevel Level { get; set; }
        public PlacementTest? PlacementTest { get; set; }
        public Guid? PlacementTestId { get; set; }

        public PlacementTestGroupResult? PlacementTestGroupResult { get; set; }
        public Guid? PlacementTestGroupResultId { get; set; }
        public StepFlow? StepFlow { get; set; }
        public Guid? StepFlowId { get; set; }
        public ActionFlow? ActionFlow { get; set; }
        public Guid? ActionFlowId { get; set; }
        public ICollection<PlacementTestAnswer> PlacementTestAnswers { get; set; } = new List<PlacementTestAnswer>();
        public ICollection<SectionGroupResult> SectionGroupResults { get; set; } = new List<SectionGroupResult>();
    }
}
