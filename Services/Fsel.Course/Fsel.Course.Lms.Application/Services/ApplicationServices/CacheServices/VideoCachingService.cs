// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities;

    public interface IVideoCachingService : IEntityCachingService<Video>
    {
    }

    public class VideoCachingService : EntityCachingService<Video>, IVideoCachingService
    {
        public VideoCachingService(ICacheService<Video> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(Video),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(Video)}";
    }
}
