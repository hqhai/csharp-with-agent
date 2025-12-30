// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public interface IProgramSkillScoresCachingService : IEntityCachingService<IList<SkillScores>>
    {
    }

    public class ProgramSkillScoresCachingService : EntityCachingService<IList<SkillScores>>, IProgramSkillScoresCachingService
    {
        public ProgramSkillScoresCachingService(ICacheService<IList<SkillScores>> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "ProgramService",
            "Entity",
            nameof(SkillScores),
        };

        public override string Prefix => $"ProgramService:Entity:{nameof(SkillScores)}";
    }
}
