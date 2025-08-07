// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class UserSearchAdminModel : BaseModel
    {
        public string? FullName { get; set; }

        public string? UserName { get; set; }

        public DateTime? Birthday { get; set; }

        public string? Email { get; set; }

        public EnumGender? Gender { get; set; }

        public string? PhoneNumber { get; set; }

        public Guid? ManageUserId { get; set; }

        public Guid? GroupId { get; set; }

        public EnumUserStatus? Status { get; set; }
    }
}
