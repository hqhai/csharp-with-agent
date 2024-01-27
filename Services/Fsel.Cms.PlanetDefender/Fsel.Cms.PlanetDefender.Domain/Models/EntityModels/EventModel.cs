// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;

    public class EventModel : BaseModel
    {
        public string? Title { get; set; }

        public DateTime StartDate { get; set; }

        public bool Status { get; set; }

        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public EnumEventType Type { get; set; }
    }
}
