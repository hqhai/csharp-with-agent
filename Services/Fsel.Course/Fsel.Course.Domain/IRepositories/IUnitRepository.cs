// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Shared.Enums;
using Unit = Fsel.Course.Domain.Entities.Unit;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IUnitRepository : IRepository<Unit>
    {
        Task<bool> IsUnitUsed(Guid id);

        Task<List<Unit>?> GetListAsync(IList<Guid>? ids, Guid? studentId);

        //Task<List<Unit>?> GetListAsync(Guid? studentId, Guid courseId, EnumLearnProcessType type);

        Task<IList<UnitModel>> GetListAsync(Guid? studentId, Guid courseId, EnumLearnProcessType type);
    }
}
