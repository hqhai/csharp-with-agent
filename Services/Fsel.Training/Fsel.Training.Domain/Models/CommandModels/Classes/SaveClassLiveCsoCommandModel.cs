// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.CommandModels.Classes
{
    using Fsel.Core.Base.BaseModels;

    public class SaveClassLiveCsoCommandModel : BaseCommandModel
    {
        public string? AccessLink { get; set; }
        public string? Note { get; set; }
    }
}
