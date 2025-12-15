// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Models.EntityModels;

    public interface IVideoTimeCodeModelCachingService : IEntityCachingService<VideoTimeCodeModel>
    {
    }

    public class VideoTimeCodeModelCachingService : EntityCachingService<VideoTimeCodeModel>, IVideoTimeCodeModelCachingService
    {
        public VideoTimeCodeModelCachingService(ICacheService<VideoTimeCodeModel> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(VideoTimeCodeModel),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(VideoTimeCodeModel)}";
    }
}
