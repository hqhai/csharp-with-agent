// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Services.UserServices.Models
{
    using System;

    public class HumanProfileModel
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Code { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? AvatarPath { get; set; }
        public string? Role { get; set; }
        public Guid? UserId { get; set; }
    }
}
