// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.FlowModels
{
    public class ActionFlowModel
    {
        public Guid Id { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid FromStepFlowId { get; set; }
        public Guid ToStepFlowId { get; set; }
        public int StartPercent { get; set; }
        public int EndPercent { get; set; }
        public StepFlowModel? FromStepFlow { get; set; }
        public StepFlowModel? ToStepFlow { get; set; }
    }
}
