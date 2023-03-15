// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Humans
{
    using System;

    public class UpdateHumanCommandModel
    {
        public string? FullName { get; set; }

        public DateTime? Birthday { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
        public string? Email { get; set; }

        public string? AvatarPath { get; set; }

        public Guid UserId { get; set; }
    }
}
