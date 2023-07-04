// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class ClassLiveWorkFlowPlan : Entity
    {
        public DateTime? LiveDate { get; set; }

        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int VoteNumber { get; set; }

        public bool IsActive { get; set; }
        public Guid LiveTimeFrameId { get; set; }
        public ClassLiveWorkFlow? ClassLiveWorkFlow { get; set; }
        public Guid ClassLiveWorkFlowId { get; set; }
    }
}
