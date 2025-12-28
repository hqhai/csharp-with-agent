// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices.BuildModules
{
    using Fsel.Common.Caching;
    using Fsel.Course.Domain.Models.EntityModels.CachingModels;

    public interface IModuleCourseCachingService : IEntityCachingService<ModuleObjectModel>
    {
    }

    public class ModuleCourseCachingService : EntityCachingService<ModuleObjectModel>, IModuleCourseCachingService
    {
        public ModuleCourseCachingService(ICacheService<ModuleObjectModel> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string>
        {
            "CourseService",
            "Entity",
            nameof(ModuleObjectModel),
        };

        public override string Prefix => $"CourseService:Entity:{nameof(ModuleObjectModel)}";
    }
}
