// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Models.EntityModels.CachingModels;

    public interface ITimeCodeQuestionCachingService : IEntityCachingService<IList<TimeCodeQuestionModel>>
    {
    }

    public class TimeCodeQuestionCachingService : EntityCachingService<IList<TimeCodeQuestionModel>>, ITimeCodeQuestionCachingService
    {
        public TimeCodeQuestionCachingService(ICacheService<IList<TimeCodeQuestionModel>> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(TimeCodeQuestionModel),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(TimeCodeQuestionModel)}";
    }
}
