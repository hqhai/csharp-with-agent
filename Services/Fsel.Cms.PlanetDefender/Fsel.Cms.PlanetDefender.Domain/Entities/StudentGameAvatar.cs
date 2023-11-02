// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using Fsel.Core.Entities;

    public class StudentGameAvatar : Entity
    {
        public bool IsActive { get; set; }

        public Guid AvatarImageId { get; set; }

        public Guid StudentGameInfoId { get; set; }

        public AvatarImage? AvatarImage { get; set; }

        public StudentGameInfo? StudentGameInfo { get; set; }
    }
}
