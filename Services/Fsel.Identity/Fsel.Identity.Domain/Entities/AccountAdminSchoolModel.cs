// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using Fsel.Shared.Enums;

    public class AccountAdminSchoolModel
    {
        public Guid Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? UserName { get; set; }

        public string? SchoolName { get; set; }

        public string? LocalId { get; set; }

        public string? City { get; set; }

        public string? EventCode { get; set; }

        public EnumUserStatus? Status { get; set; }

        public string? DefaultPassword { get; set; }
    }
}
