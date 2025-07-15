// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Fsel.Identity.Domain.Models.CommandModels.UserGroup
{
    public class RemoveUserFromGroupCommandModel
    {
        [Required]
        public Guid GroupId { get; set; }

        [Required]
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
