// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Fsel.Identity.Domain.Entities;

    public interface IUserRoleRepository
    {
        //IQueryable<IdentityUserRole<Guid>> GetQuery();
        IQueryable<UserRole> GetQuery();

        Task<bool> AddAsync(UserRole userRole);

        Task<bool> AddRangeAsync(IEnumerable<UserRole> userRoles);

        Task<bool> UpdateAsync(UserRole userRole);

        Task<bool> UpdateRangeAsync(IEnumerable<UserRole> userRoles);

        Task<bool> DeleteAsync(UserRole userRole);
    }
}
