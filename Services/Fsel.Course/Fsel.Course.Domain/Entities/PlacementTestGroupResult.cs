// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class PlacementTestGroupResult : Entity, IModuleLifeCycle
    {
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public double Percent { get; set; }
        public EnumPlacementTestLevel? ProcessLevel { get; set; }
        public EnumPlacementTestLevel? CompletionLevel { get; set; }
        public EnumCourseLevel? SuggetLevel { get; set; }
        public EnumCourseLevel? ChooseLevel { get; set; }
        public DateTime? NewDate { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }
        public ICollection<PlacementTestResult> PlacementTestResults { get; set; } = new List<PlacementTestResult>();
    }
}
