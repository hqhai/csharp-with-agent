// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.ForbiddenWords
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateForbiddenWordCommandModel : BaseCommandModel
    {
        public string? Word { get; set; }
        public string? Description { get; set; }
    }
}
