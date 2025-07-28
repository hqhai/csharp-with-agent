// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IUnitRepository : IRepository<Unit>
    {
        Task<bool> IsUnitUsed(Guid id);

        Task<bool> IsUsingByClient(Guid id);

        Task<List<Unit>?> GetListAsync(IList<Guid>? ids, Guid? studentId);

        Task<Unit?> GetIncludeAsync(Guid? id, Guid courseId, Guid? studentId);
    }
}
