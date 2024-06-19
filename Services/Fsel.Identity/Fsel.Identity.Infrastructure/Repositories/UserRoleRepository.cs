// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System;
    using System.Linq;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.AspNetCore.Identity;

    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly UserDbContext _userDbContext;

        public UserRoleRepository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public virtual IQueryable<IdentityUserRole<Guid>> GetQuery()
        {
            try
            {
                return _userDbContext.UserRoles.AsQueryable();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
