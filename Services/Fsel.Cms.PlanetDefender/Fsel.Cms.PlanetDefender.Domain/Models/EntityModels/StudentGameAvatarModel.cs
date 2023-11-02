// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class StudentGameAvatarModel : BaseModel
    {
        public bool IsActive { get; set; }

        public Guid AvatarImageId { get; set; }

        public Guid StudentGameInfoId { get; set; }
    }
}
