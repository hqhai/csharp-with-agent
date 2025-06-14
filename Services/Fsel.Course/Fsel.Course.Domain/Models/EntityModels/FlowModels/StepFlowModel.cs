// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.FlowModels
{
    using Fsel.Course.Domain.Enums;

    public class StepFlowModel
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public EnumStepFlowType Type { get; set; }
        public string? LevelName { get; set; }
        public Guid? ParentId { get; set; }
        public Guid LevelId { get; set; }
        public IList<ActionFlowModel> ChildActionFlows { get; set; } = new List<ActionFlowModel>();
    }
}
