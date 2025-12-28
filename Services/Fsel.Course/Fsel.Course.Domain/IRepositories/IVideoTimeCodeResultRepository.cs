// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;

    public interface IVideoTimeCodeResultRepository : IRepository<VideoTimeCodeResult>
    {
        Task<IList<SkillScores>> GetSkillScoreResultsAsync(Guid courseId, Guid studentId, EnumTimeCodeType timeCodeType);
    }
}
