// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Application.Services.UserService.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UserModel : BaseModel
    {
        public EnumGender? Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Code { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? AvatarPath { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
