using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class RoleClaimRepository : IRoleClaimRepository
    {
        private readonly UserDbContext _userDbContext;

        public RoleClaimRepository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public virtual IQueryable<RoleClaim> Queryable
        {
            get
            {
                IQueryable<RoleClaim> queryable = _userDbContext.RoleClaims.AsQueryable();
                return queryable;
            }
        }

        public virtual async Task<IList<RoleClaim>> GetClaimsByRole(Guid roleId, CancellationToken cancellationToken)
        {
            try
            {
                return await _userDbContext.RoleClaims.Where(x => x.RoleId == roleId && x.Status).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<IList<RoleClaim>> GetClaimsByRole(Guid roleId, IList<string> claimValues, CancellationToken cancellationToken)
        {
            try
            {
                return await _userDbContext.RoleClaims.Where(x => x.RoleId == roleId && x.Status && x.ClaimValue != null && claimValues.Contains(x.ClaimValue)).ToArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task UpdateListAsync(IList<RoleClaim> updateEntities)
        {
            ArgumentNullException.ThrowIfNull(updateEntities);
            try
            {
                await _userDbContext.RoleClaims.BulkUpdateAsync(updateEntities);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task AddRangeAsync(IList<RoleClaim> roleClaims)
        {
            try
            {
                await _userDbContext.RoleClaims.BulkInsertAsync(roleClaims);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task DeleteListAsync(IList<RoleClaim> deleteEntities)
        {
            ArgumentNullException.ThrowIfNull(deleteEntities);
            try
            {
                await _userDbContext.RoleClaims.BulkDeleteAsync(deleteEntities);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
