// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class AvatarImageModel : BaseModel
    {
        public bool IsActive { get; set; }

        public string? FilePath { get; set; }

        public int? Level { get; set; }
    }
}
