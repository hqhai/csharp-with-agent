using Fsel.Common.ActionResults;
using Fsel.Core.Entities;

namespace Fsel.Core.Base.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        IQueryable<T> Queryable { get; }
        IUnitOfWork UnitOfWork { get; }

        Task<T?> GetByIdAsync(Guid id, int? siteId = null);

        Task<T?> GetIncludeByIdAsync(Guid id, int? siteId = null);

        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, int? siteId = null);

        Task<bool> AnyAsync(Guid id, int? siteId = null);

        bool IsIdsInValid(IEnumerable<Guid> ids, int? siteId = null);

        Task<bool> AnyGuidAsync(Guid id, int? siteId = null);

        T Add(T newEntity);

        Task AddList(IEnumerable<T> newEntities);

        T Update(T updateEntity);

        void UpdateList(IEnumerable<T> updateEntities);

        Task<bool> DeleteAsync(T deleteEntity);

        Task ExecuteTransactionAsync(Func<Task<VoidMethodResult>> action);
    }
}
