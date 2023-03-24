// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Enums;

    public class HumanModel : BaseModel
    {
        public string? FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public EnumGender? Gender { get; set; }
        public string? Email { get; set; }
        public string? AvatarPath { get; set; }
    }
}
