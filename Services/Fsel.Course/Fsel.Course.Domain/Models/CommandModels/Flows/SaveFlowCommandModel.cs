// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Flows
{
    using System;
    using System.Collections.Generic;

    public class SaveFlowCommandModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public bool IsModified { get; set; }
        public int FromAge { get; set; }
        public int ToAge { get; set; }
        public object? Config { get; set; }
        public SaveStepFlowCommandModel? StepFlow { get; set; }
    }

    public class SaveStepFlowCommandModel
    {
        public Guid? Id { get; set; }
        public Guid? LevelId { get; set; }
        public string? LevelCode { get; set; }
        public IList<SaveActionFlowCommandModel> ActionFlows { get; set; } = new List<SaveActionFlowCommandModel>();
    }

    public class SaveActionFlowCommandModel
    {
        public Guid? Id { get; set; }
        public int StartPercent { get; set; }
        public int EndPercent { get; set; }
        public SaveStepFlowCommandModel? StepFlow { get; set; }
    }
}
