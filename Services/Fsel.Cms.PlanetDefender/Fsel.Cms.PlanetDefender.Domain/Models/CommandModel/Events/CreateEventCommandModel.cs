// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.NewsAndUpdates
{
    using Fsel.Cms.PlanetDefender.Domain.Enums;

    public class CreateEventCommandModel
    {
        public string? Title { get; set; }

        public DateTime StartDate { get; set; }

        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public EnumEventType Type { get; set; }
    }
}
