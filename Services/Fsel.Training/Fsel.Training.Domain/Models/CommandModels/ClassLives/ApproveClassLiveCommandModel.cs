// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLives
{
    using Fsel.Core.Base.BaseModels;

    public class ApproveClassLiveCommandModel : BaseCommandModel
    {
        public bool IsAccept { get; set; }
        public string? Description { get; set; }
    }
}
