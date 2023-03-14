// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;

    public class HumanModel : BaseEntityModel
    {
        public string? FullName { get; set; }

        public DateTime? Birthday { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? AvatarPath { get; set; }

        public User? User { get; set; }

        public Guid UserId { get; set; }
    }
}
