// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.FlowModels
{
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class FlowModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedFullName { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public EnumFlowType Type { get; set; }
        public int FromAge { get; set; }
        public int ToAge { get; set; }
        public bool IsUsedInPlacementTest { get; set; }
        public object? Config { get; set; }
        public EnumStatus Status { get; set; }
        public IList<StepFlowModel> StepFlows { get; set; } = new List<StepFlowModel>();
    }
}
