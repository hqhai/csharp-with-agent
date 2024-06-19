// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Microsoft.AspNetCore.Identity;

    public interface IUserRoleRepository
    {
        IQueryable<IdentityUserRole<Guid>> GetQuery();
    }
}
