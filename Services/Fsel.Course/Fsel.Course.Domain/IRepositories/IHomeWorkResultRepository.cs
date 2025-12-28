// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public interface IHomeWorkResultRepository : IRepository<HomeWorkResult>
    {
        Task<bool> GetCheckByIdsAsync(IEnumerable<Guid>? ids);

        Task<List<HomeWorkResult>> GetHomeWorkResultsAsync(Guid courseId, Guid studentId);

        Task<IList<SkillScores>> GetSkillScoreResultsAsync(Guid courseId, Guid studentId);
    }
}
