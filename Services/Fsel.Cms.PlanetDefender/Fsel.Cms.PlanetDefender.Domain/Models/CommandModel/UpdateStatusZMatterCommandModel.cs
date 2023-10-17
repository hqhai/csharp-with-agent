// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateStatusZMatterCommandModel : BaseCommandModel
    {
        public bool IsActive { get; set; }
    }
}
