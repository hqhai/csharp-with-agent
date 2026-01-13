// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public interface IClassForumResultRepository : IRepository<ClassForumResult>
    {
        Task<IList<SkillScores>> GetSkillScoreResultsAsync(Guid courseId, Guid studentId);

        Task<IList<ClassForumResult>> GetClassForumResultsAsync(Guid courseId, Guid studentId);
    }
}
