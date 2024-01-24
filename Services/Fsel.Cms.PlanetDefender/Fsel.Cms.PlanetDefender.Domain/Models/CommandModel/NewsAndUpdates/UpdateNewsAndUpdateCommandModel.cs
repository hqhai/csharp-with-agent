// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.NewsAndUpdates
{
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;

    public class UpdateNewsAndUpdateCommandModel : BaseQueryModel
    {
        public string? Titile { get; set; }

        public DateTime StartDate { get; set; }

        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public EnumNewsAndUpdateType Type { get; set; }
    }
}
