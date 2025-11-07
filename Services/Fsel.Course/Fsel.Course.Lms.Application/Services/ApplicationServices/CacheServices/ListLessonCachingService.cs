// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Common.Caching;
    using Domain.Models.EntityModels.V1i2;

    public interface IListLessonCachingService : IEntityCachingService<List<LessonModel>>
    {

    }
    public class ListLessonCachingService : EntityCachingService<List<LessonModel>>, IListLessonCachingService
    {
        public ListLessonCachingService(ICacheService<List<LessonModel>> cacheService) : base(cacheService)
        {
        }
        public override List<string> Tags => new List<string>
        {
            "LessonService",
            "Model",
            nameof(List<LessonModel>),
        };

        public override string Prefix => $"LessonService:Model:{nameof(List<LessonModel>)}";
    }
}
