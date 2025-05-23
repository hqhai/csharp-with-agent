// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly UserDbContext _userDbContext;

        public UserRoleRepository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public virtual IQueryable<UserRole> GetQuery()
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

        public virtual async Task<bool> DeleteAsync(UserRole userRole)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _userDbContext.Database.BeginTransactionAsync();
                try
                {
                    _userDbContext.UserRoles.Remove(userRole);
                    await _userDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public virtual async Task<bool> AddAsync(UserRole userRole)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _userDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _userDbContext.UserRoles.AddAsync(userRole);
                    await _userDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public virtual async Task<bool> AddRangeAsync(IEnumerable<UserRole> userRoles)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _userDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _userDbContext.UserRoles.AddRangeAsync(userRoles);
                    await _userDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public virtual async Task<bool> UpdateAsync(UserRole userRole)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _userDbContext.Database.BeginTransactionAsync();
                try
                {
                    _userDbContext.UserRoles.Update(userRole);
                    await _userDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public virtual async Task<bool> UpdateRangeAsync(IEnumerable<UserRole> userRoles)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _userDbContext.Database.BeginTransactionAsync();
                try
                {
                    _userDbContext.UserRoles.UpdateRange(userRoles);
                    await _userDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
