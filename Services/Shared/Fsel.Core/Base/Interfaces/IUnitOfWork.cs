namespace Fsel.Core.Base.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task<Guid> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}