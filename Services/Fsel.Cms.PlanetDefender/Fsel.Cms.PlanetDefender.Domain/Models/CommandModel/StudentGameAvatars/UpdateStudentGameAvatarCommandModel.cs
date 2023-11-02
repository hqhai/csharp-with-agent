// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameAvatars
{
    public class UpdateStudentGameAvatarCommandModel
    {
        public bool IsActive { get; set; }

        public Guid AvatarImageId { get; set; }
    }
}
