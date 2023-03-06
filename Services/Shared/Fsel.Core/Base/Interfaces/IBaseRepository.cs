using Fsel.Common.ActionResults;
using Fsel.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Core.Base.Interfaces
{
    public interface IRepository<T> where T : Entity
    {
        IQueryable<T> Queryable { get; }
        IUnitOfWork UnitOfWork { get; }

        Task<T?> GetByIdAsync(Guid id, int? siteId = null);

        Task<bool> AnyAsync(Guid id, int? siteId = null);

        Task<bool> AnyGuidAsync(Guid id, int? siteId = null);

        T Add(T newEntity);

        T Update(T updateEntity);

        Task<bool> DeleteAsync(T deleteEntity);

        Task ExecuteTransactionAsync(Func<Task<VoidMethodResult>> action);
    }
}
