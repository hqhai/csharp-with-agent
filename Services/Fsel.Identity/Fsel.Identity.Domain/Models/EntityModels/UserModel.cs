// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UserModel : BaseModel
    {
        public string? Code { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarPath { get; set; }
        public DateTime? Birthday { get; set; }
        public EnumGender? Gender { get; set; }
        public string? Address { get; set; }
        public string? School { get; set; }
        public string? SchoolName { get; set; }
        public Guid? CourseId { get; set; }
        public string? UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public EnumUserStatus? Status { get; set; }
    }
}
