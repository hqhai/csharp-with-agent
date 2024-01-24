// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;

    public class NewsAndUpdateModel : BaseModel
    {
        public string? Titile { get; set; }

        public DateTime StartDate { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public string? FilePath { get; set; }

        public EnumNewsAndUpdateType Type { get; set; }
    }
}
