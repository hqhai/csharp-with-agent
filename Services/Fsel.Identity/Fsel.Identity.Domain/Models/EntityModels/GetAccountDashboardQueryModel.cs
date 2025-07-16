// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class GetAccountDashboardQueryModel
    {
        public Guid Id { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? FullName { get; set; }

        public string? UserName { get; set; }

        public string? Role { get; set; }

        public string? EventCode { get; set; }

        public EnumUserStatus? Status { get; set; }

        public string? DefaultPassword { get; set; }
    }

    public class GetUserRoleQueryModel
    {
        public Guid RoleId { get; set; }
        public string? RoleName { get; set; }
    }

}
