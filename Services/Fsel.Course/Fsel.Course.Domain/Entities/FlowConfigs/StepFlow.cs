// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.FlowConfigs
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;

    public class StepFlow : Entity
    {
        public EnumStepFlowType Type { get; set; }

        public Flow? Flow { get; set; }
        public Guid? FlowId { get; set; }

        public StepFlow? ParentFlowStep { get; set; }
        public Guid? ParentId { get; set; }

        public Level? Level { get; set; }
        public Guid LevelId { get; set; }
        public ICollection<PlacementTestResult> PlacementTestResults { get; set; } = new List<PlacementTestResult>();
        public ICollection<StepFlow> StepFlows { get; set; } = new List<StepFlow>();
        public ICollection<ActionFlow> ChildActionFlows { get; set; } = new List<ActionFlow>();
        public ICollection<ActionFlow> ParentActionFlows { get; set; } = new List<ActionFlow>();
        public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
