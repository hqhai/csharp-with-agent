// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Entities.SkillScoresConfigs;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.IRepositories
{
    public interface IVideoTimeCodeRepository : IRepository<VideoTimeCode>
    {
        Task<IList<SkillScores>> GetSkillScoresAsync(List<Guid> videoIds, EnumTimeCodeType timeCodeType);
    }
}
