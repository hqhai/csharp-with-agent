// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
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

        public virtual IQueryable<GetAccountDashboardQueryModel> GetUsersByRoles(IList<EnumRole> roles)
        {
            try
            {
                var roleNames = roles.Select(r => r.ToString()).ToList();

                return (from a in _userDbContext.Users
                        join b in _userDbContext.UserRoles on a.Id equals b.UserId
                        join c in _userDbContext.Roles on b.RoleId equals c.Id
                        where !string.IsNullOrEmpty(c.Name) && roleNames.Contains(c.Name!)
                        select new GetAccountDashboardQueryModel
                        {
                            Id = a.Id,
                            CreatedDate = a.CreatedDate,
                            FullName = a.FullName,
                            UserName = a.UserName,
                            Status = a.Status,
                            Role = c.Name,
                            DefaultPassword = a.DefaultPassword
                        }).AsQueryable();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
