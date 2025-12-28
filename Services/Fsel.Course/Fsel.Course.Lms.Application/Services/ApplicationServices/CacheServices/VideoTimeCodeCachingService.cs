// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using System.Collections.Generic;
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Entities;

    public interface IVideoTimeCodeCachingService : IEntityCachingService<IList<VideoTimeCode>>
    {
    }

    public class VideoTimeCodeCachingService : EntityCachingService<IList<VideoTimeCode>>, IVideoTimeCodeCachingService
    {
        public VideoTimeCodeCachingService(ICacheService<IList<VideoTimeCode>> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(VideoTimeCode),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(VideoTimeCode)}";
    }
}
