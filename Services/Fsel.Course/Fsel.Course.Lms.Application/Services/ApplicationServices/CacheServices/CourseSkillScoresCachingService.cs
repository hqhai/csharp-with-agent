// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;

    public interface ICourseSkillScoresCachingService : IEntityCachingService<IList<SkillScores>>
    {
    }

    public class CourseSkillScoresCachingService : EntityCachingService<IList<SkillScores>>, ICourseSkillScoresCachingService
    {
        public CourseSkillScoresCachingService(ICacheService<IList<SkillScores>> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(SkillScores),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(SkillScores)}";
    }
}
