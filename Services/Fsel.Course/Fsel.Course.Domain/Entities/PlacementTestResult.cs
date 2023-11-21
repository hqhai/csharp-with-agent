// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Shared.Enums;

    public class PlacementTestResult : BaseResultScore
    {
        public double TotalQuestion { get; set; }
        public double CountQuestion { get; set; }
        public EnumPlacementTestLevel Level { get; set; }
        public PlacementTest? PlacementTest { get; set; }
        public Guid? PlacementTestId { get; set; }
        public ICollection<PlacementTestAnswer> PlacementTestAnswers { get; set; } = new List<PlacementTestAnswer>();
        public ICollection<SectionGroupResult> SectionGroupResults { get; set; } = new List<SectionGroupResult>();
    }
}
