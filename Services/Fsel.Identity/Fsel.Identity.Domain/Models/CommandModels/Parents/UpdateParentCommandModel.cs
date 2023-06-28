// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Parents
{
    using System;
    using Fsel.Identity.Domain.Enums;

    public class UpdateParentCommandModel
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public EnumGender? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
