// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class AvatarImageModel : BaseModel
    {
        public string? FilePath { get; set; }

        public int? Level { get; set; }
    }
}
