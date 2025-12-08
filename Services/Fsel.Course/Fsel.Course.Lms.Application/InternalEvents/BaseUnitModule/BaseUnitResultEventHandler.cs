// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices;
    using Microsoft.EntityFrameworkCore;

    public interface IUnitResultUpdater
    {
        Task UpdateUnitResultAsync(UnitResult unitResult, Guid unitModuleId, CancellationToken cancellationToken);
    }

    public class BaseUnitResultEventHandler : IUnitResultUpdater
    {
        private readonly IUnitModuleCachingService _unitModuleCachingService;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ITestRepository _testRepository;
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly IUnitItemInitializerFactory _unitItemInitializerFactory;

        public BaseUnitResultEventHandler(IUnitModuleCachingService unitModuleCachingService,
            IUnitModuleRepository unitModuleRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ITestRepository testRepository,
            ITestGroupResultRepository testGroupResultRepository,
            IUnitItemInitializerFactory unitItemInitializerFactory)
        {
            _unitModuleCachingService = unitModuleCachingService;
            _unitModuleRepository = unitModuleRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _testRepository = testRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _unitItemInitializerFactory = unitItemInitializerFactory;
        }

        public async Task UpdateUnitResultAsync()
        {
        }

        public async Task UpdateUnitResultAsync(UnitResult unitResult, Guid unitModuleId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unitResult);
            var unitModules = await GetUnitModulesAsync(unitResult.UnitId);
            var currentModule = FindCurrentModule(unitModules, unitModuleId);
            if (currentModule == null)
            {
                return;
            }

            var nextModule = GetNextModule(unitModules, currentModule);
            if (nextModule != null)
            {
                await UpdateNewResultCourseModule(nextModule, unitResult, cancellationToken);
                return;
            }
            await UpdateUnitResultAsync();
        }

        private async Task UpdateNewResultCourseModule(UnitModule nextModule, UnitResult unitResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);
            var initializer = _unitItemInitializerFactory.Get(nextModule.UnitConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, unitResult, cancellationToken);
            }
            return;
        }

        private static UnitModule? FindCurrentModule(IList<UnitModule> unitModules, Guid? unitModuleId)
        {
            if (!unitModuleId.HasValue)
            {
                return null;
            }
            return unitModules.FirstOrDefault(m => m.Id == unitModuleId);
        }

        private static UnitModule? GetNextModule(IList<UnitModule> unitModules, UnitModule currentModule)
        {
            return unitModules.Where(m => m.OpenOrder > currentModule.OpenOrder)
                              .OrderBy(m => m.OpenOrder)
                              .FirstOrDefault();
        }

        public async Task<IList<UnitModule>> GetUnitModulesAsync(Guid id)
        {
            return await _unitModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var unitModules = await _unitModuleRepository.ReadQueryable
                                                             .Where(x => x.UnitId == id)
                                                             .ToListAsync(_);

                return unitModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
