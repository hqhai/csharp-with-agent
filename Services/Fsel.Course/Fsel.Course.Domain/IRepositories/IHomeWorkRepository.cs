// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IHomeWorkRepository : IRepository<HomeWork>
    {
        Task<bool> IsHomeWorkUsed(Guid? id);

        Task<HomeWorkModel?> GetIncludeAllAsync(Guid? id);

        Task<IList<HomeWork>> GetListAsync(LessonResult lessonResult);

        Task<HomeWork?> GetAsync(HomeWorkResult homeWorkResult);

        Task<bool> IsUsingByClient(Guid id);

        Task<IDictionary<Guid, HomeWork>> GetHomeWorkDicAsync(IList<Guid>? originalIds);

        Task<(IDictionary<Guid, (HomeWork, HomeWorkResult)>, IDictionary<Guid, HomeWork>)> BuildHomeWorkLookupsAsync(LessonResult lessonResult, IList<LessonModule> lessonModules);
    }
}
