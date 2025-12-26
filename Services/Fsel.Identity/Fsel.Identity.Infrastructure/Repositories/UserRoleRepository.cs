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
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Core.Entities;

    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly UserDbContext _userDbContext;

        public UserRoleRepository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public IQueryable<UserRoleEntity> Queryable
        {
            get
            {
                IQueryable<UserRoleEntity> queryable = _userDbContext.UserRoles.AsQueryable();
                return queryable;
            }
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
                // Kiểm tra xem đã có transaction chưa
                if (_userDbContext.Database.CurrentTransaction != null)
                {
                    // Đã có transaction, chỉ cần remove và save
                    _userDbContext.UserRoles.Remove(userRole);
                    await _userDbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Chưa có transaction, tạo mới
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
                }
            });
        }

        public virtual async Task<bool> AddAsync(UserRole userRole)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                // Kiểm tra xem đã có transaction chưa
                if (_userDbContext.Database.CurrentTransaction != null)
                {
                    // Đã có transaction, chỉ cần add và save
                    await _userDbContext.UserRoles.AddAsync(userRole);
                    await _userDbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Chưa có transaction, tạo mới
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
                }
            });
        }

        public virtual async Task<bool> AddRangeAsync(IEnumerable<UserRole> userRoles)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                // Kiểm tra xem đã có transaction chưa
                if (_userDbContext.Database.CurrentTransaction != null)
                {
                    // Đã có transaction, chỉ cần add range và save
                    await _userDbContext.UserRoles.AddRangeAsync(userRoles);
                    await _userDbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Chưa có transaction, tạo mới
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
                }
            });
        }

        public virtual async Task<bool> UpdateAsync(UserRole userRole)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                // Kiểm tra xem đã có transaction chưa
                if (_userDbContext.Database.CurrentTransaction != null)
                {
                    // Đã có transaction, chỉ cần update và save
                    _userDbContext.UserRoles.Update(userRole);
                    await _userDbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Chưa có transaction, tạo mới
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
                }
            });
        }

        public virtual async Task<bool> UpdateRangeAsync(IEnumerable<UserRole> userRoles)
        {
            var strategy = _userDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                // Kiểm tra xem đã có transaction chưa
                if (_userDbContext.Database.CurrentTransaction != null)
                {
                    // Đã có transaction, chỉ cần update range và save
                    _userDbContext.UserRoles.UpdateRange(userRoles);
                    await _userDbContext.SaveChangesAsync();
                    return true;
                }
                else
                {
                    // Chưa có transaction, tạo mới
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
                }
            });
        }

        public virtual IQueryable<GetAccountDashboardQueryModel> GetUsersByRoles(IList<EnumRole> roles)
        {
            try
            {
                var roleNames = roles.Select(r => r.ToString()).ToList();

                return (from a in _userDbContext.Users
                        join b in _userDbContext.UserRoles on a.Id equals b.UserId
                        join c in _userDbContext.Roles on b.RoleId equals c.Id
                        let em = _userDbContext.EventManagers
                           .Where(x => x.UserId == a.Id && x.CompetitionEvent != null &&
                                       (c.Name != EnumRole.EducationDivision.ToString()
                                           ? !x.CompetitionEvent.ParentEventId.HasValue
                                           : x.CompetitionEvent.Category == EnumCompetitionEventCategory.Student))
                           .Select(x => new
                           {
                               x.CompetitionEvent!.EventCode
                           })
                           .FirstOrDefault()
                        where !string.IsNullOrEmpty(c.Name) && roleNames.Contains(c.Name!)
                        select new GetAccountDashboardQueryModel
                        {
                            Id = a.Id,
                            CreatedDate = a.CreatedDate,
                            FullName = a.FullName,
                            UserName = a.UserName,
                            Status = a.Status,
                            Role = c.Name,
                            DefaultPassword = a.DefaultPassword,
                            EventCode = em != null ? em.EventCode : null
                        }).OrderByDescending(x => x.CreatedDate).AsQueryable();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public virtual async Task<GetUserRoleQueryModel> GetRoleIdsAndNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await (from ur in _userDbContext.UserRoles
                                    join r in _userDbContext.Roles on ur.RoleId equals r.Id
                                    where ur.UserId == userId
                                    select new GetUserRoleQueryModel
                                    {
                                        RoleId = ur.RoleId,
                                        RoleName = r.Name
                                    })
                                  .FirstOrDefaultAsync(cancellationToken);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
