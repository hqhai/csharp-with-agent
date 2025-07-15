// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Microsoft.AspNetCore.Identity;

    public interface IUserRoleRepository
    {
        IQueryable<IdentityUserRole<Guid>> GetQuery();

        IQueryable<GetAccountDashboardQueryModel> GetUsersByRoles(IList<EnumRole> roles);
    }
}
