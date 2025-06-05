// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Entities;
    using Microsoft.AspNetCore.Identity;

    public interface IUserRoleRepository
    {
        IQueryable<IdentityUserRole<Guid>> GetQuery();
        IQueryable<UserRoleEntity> Queryable { get; }
    }
}
