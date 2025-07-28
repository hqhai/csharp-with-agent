// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.FlowConfigs
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;

    public class ActionFlow : Entity
    {
        public StepFlow? FromStepFlow { get; set; }
        public Guid FromStepFlowId { get; set; }
        public StepFlow? ToStepFlow { get; set; }
        public Guid ToStepFlowId { get; set; }

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int StartPercent { get; set; }

        [Range(1, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int EndPercent { get; set; }

        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SystemAutoCode { get; set; }

        public ICollection<PlacementTestResult> PlacementTestResults { get; set; } = new List<PlacementTestResult>();
    }
}
