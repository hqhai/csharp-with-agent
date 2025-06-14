// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.FlowConfigs
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class Flow : Entity
    {
        public EnumFlowType Type { get; set; }

        [Range(0, 150, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int FromAge { get; set; }

        [Range(1, 150, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int ToAge { get; set; }

        public EnumStatus Status { get; set; }

        public Category? Category { get; set; }
        public Guid ProgramId { get; set; }
        public ICollection<PlacementTestGroupResult> PlacementTestGroupResult { get; set; } = new List<PlacementTestGroupResult>();
        public ICollection<StepFlow> StepFlows { get; set; } = new List<StepFlow>();
    }
}
