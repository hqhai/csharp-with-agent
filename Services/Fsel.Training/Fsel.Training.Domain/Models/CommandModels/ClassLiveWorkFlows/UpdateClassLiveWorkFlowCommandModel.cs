// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLiveWorkFlows
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateClassLiveWorkFlowCommandModel : BaseCommandModel
    {
        public bool IsActice { get; set; }
    }
}
