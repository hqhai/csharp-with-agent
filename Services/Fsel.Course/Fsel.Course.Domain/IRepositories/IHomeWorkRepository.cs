// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Domain.Entities.V1i1;
using Fsel.Course.Domain.Enums;
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

        Task<(IList<HomeWork>, IList<HomeWorkResult>)> GetModulesListAsync(LessonResult lessonResult, Guid? homeWorkId);

        Task<(IDictionary<Guid, (HomeWork, LessonModule, HomeWorkResult)>, IDictionary<Guid, HomeWork>)> BuildHomeWorkLookupsAsync(LessonResult? lessonResult, IList<LessonModule> lessonModules);

        Task<IList<SkillScores>> GetSkillScoresAsync(List<Guid> ids);
    }
}
