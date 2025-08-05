// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UserManageModel : BaseModel
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public DateTime? Birthday { get; set; }

        public EnumGender? Gender { get; set; }

        public string? Position { get; set; }

        public Guid? ManageUserId { get; set; }

        public string? ManageUser { get; set; }

        public string? GroupName { get; set; }

        public string? UserName { get; set; }

        public EnumUserStatus? Status { get; set; }

        public Guid? RoleId { get; set; }
    }
}
