// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseCourseModule
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices;
    using Microsoft.EntityFrameworkCore;

    public interface ICourseResultUpdater
    {
        Task UpdateCourseResultAsync(CourseResult courseResult, Guid courseModuleId, CancellationToken cancellationToken);
    }

    public class BaseCourseResultEventHandler : ICourseResultUpdater
    {
        private readonly ICourseModuleCachingService _courseModuleCachingService;
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ICourseItemInitializerFactory _courseItemInitializerFactory;

        public BaseCourseResultEventHandler(ICourseModuleCachingService courseModuleCachingService,
            ICourseModuleRepository courseModuleRepository,
            ICourseResultRepository courseResultRepository,
            ICourseItemInitializerFactory courseItemInitializerFactory)
        {
            _courseModuleCachingService = courseModuleCachingService;
            _courseModuleRepository = courseModuleRepository;
            _courseResultRepository = courseResultRepository;
            _courseItemInitializerFactory = courseItemInitializerFactory;
        }

        public async Task UpdateCourseResultAsync()
        {
        }

        public async Task UpdateCourseResultAsync(CourseResult courseResult, Guid courseModuleId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(courseResult);
            var courseModules = await GetCourseModulesAsync(courseResult.CourseId);
            var currentModule = FindCurrentModule(courseModules, courseModuleId);
            if (currentModule == null)
            {
                return;
            }

            var nextModule = GetNextModule(courseModules, currentModule);
            if (nextModule != null)
            {
                await UpdateNewResultCourseModule(nextModule, courseResult, cancellationToken);
                return;
            }
            await UpdateCourseResultAsync();
        }

        private async Task UpdateNewResultCourseModule(CourseModule nextModule, CourseResult courseResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);
            var initializer = _courseItemInitializerFactory.Get(nextModule.CourseConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, courseResult, cancellationToken);
            }
            return;
        }

        private static CourseModule? FindCurrentModule(IList<CourseModule> courseModules, Guid? courseModuleId)
        {
            if (!courseModuleId.HasValue)
            {
                return null;
            }
            return courseModules.FirstOrDefault(m => m.Id == courseModuleId);
        }

        private static CourseModule? GetNextModule(IList<CourseModule> courseModules, CourseModule currentModule)
        {
            return courseModules.Where(m => m.OpenOrder > currentModule.OpenOrder)
                                .OrderBy(m => m.OpenOrder)
                                .FirstOrDefault();
        }

        public async Task<IList<CourseModule>> GetCourseModulesAsync(Guid id)
        {
            return await _courseModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var courseModules = await _courseModuleRepository.ReadQueryable
                                                             .Where(x => x.CourseId == id)
                                                             .ToListAsync(_);

                return courseModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
