// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.ClassLives
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateClassLiveCommandModel : BaseCommandModel
    {
        public bool IsActice { get; set; }
        public string? Description { get; set; }
    }
}
