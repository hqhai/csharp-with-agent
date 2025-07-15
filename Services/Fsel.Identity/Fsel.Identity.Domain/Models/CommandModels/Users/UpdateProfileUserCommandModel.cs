// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using Fsel.Shared.Enums;

    public class UpdateProfileUserCommandModel
    {
        public Guid Id { get; set; }

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime Birthday { get; set; }

        public EnumGender? Gender { get; set; }

        public string? Position { get; set; }

        public Guid? ManageUserId { get; set; }

        public Guid? GroupId { get; set; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public EnumUserStatus? Status { get; set; }
    }
}
