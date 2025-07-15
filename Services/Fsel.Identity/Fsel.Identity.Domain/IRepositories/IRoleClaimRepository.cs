namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Identity.Domain.Entities;

    public interface IRoleClaimRepository
    {
        IQueryable<RoleClaim> Queryable { get; }

        Task DeleteListAsync(IList<RoleClaim> deleteEntities);

        Task AddRangeAsync(IList<RoleClaim> roleClaims);

        Task UpdateListAsync(IList<RoleClaim> updateEntities);

        Task<IList<RoleClaim>> GetClaimsByRole(Guid roleId, CancellationToken cancellationToken);

        Task<IList<RoleClaim>> GetClaimsByRole(Guid roleId, IList<string> claimValues, CancellationToken cancellationToken);
    }
}
