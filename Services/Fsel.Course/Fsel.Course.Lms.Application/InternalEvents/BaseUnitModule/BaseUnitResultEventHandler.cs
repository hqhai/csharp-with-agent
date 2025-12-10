// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
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

        public async Task UpdateUnitResultAsync(UnitResult unitResult, IList<UnitModule> unitModules, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unitResult);
            // Tính toán lại tiến độ hoàn thành bài học
            if (!unitModules.Any())
            {
                unitResult.Percent = 100;
            }
            else
            {
                var testResults = await _testGroupResultRepository.ReadQueryable
                                                                       .Where(x => x.UnitResultId == unitResult.Id && x.Status == EnumResultStatus.Done)
                                                                       .SelectMany(x => x.TestResults)
                                                                       .ToListAsync(cancellationToken);

                var lessonResults = await _lessonResultRepository.ReadQueryable
                                                                     .Where(x => x.UnitResultId == unitResult.Id && x.Status == EnumResultStatus.Done)
                                                                     .ToListAsync(cancellationToken);
                var totalPercentModule = lessonResults.Sum(x => x.PercentModule) + testResults.Sum(x => x.PercentModule);
                var totalCorrectCount = lessonResults.Sum(x => x.CorrectCount) + testResults.Sum(x => x.CorrectCount);
                var totalCorrectTotal = lessonResults.Sum(x => x.CorrectTotal) + testResults.Sum(x => x.CorrectTotal);

                var allSkillScores = Enumerable.Empty<SkillScores>()
                 .Concat(lessonResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()))
                 .Concat(testResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()));

                var aggregatedSkillScores = allSkillScores
                  .GroupBy(s => new { s.SkillId, s.Skill })
                  .Select(g =>
                  {
                      var first = g.First();
                      return new SkillScores
                      {
                          Skill = first.Skill,
                          SkillId = first.SkillId,
                          SkillName = first.SkillName,

                          // Tổng hợp các giá trị
                          Scores = g.Sum(x => x.Scores),
                          TotalCount = g.Sum(x => x.TotalCount),
                          CorrectCount = g.Sum(x => x.CorrectCount),
                          TotalQuestion = g.Sum(x => x.TotalQuestion),
                          CountQuestion = g.Sum(x => x.CountQuestion),
                          TokenReceived = g.Sum(x => x.TokenReceived),
                          CorrectQuestion = g.Any(x => x.CorrectQuestion.HasValue)
                              ? g.Where(x => x.CorrectQuestion.HasValue).Sum(x => x.CorrectQuestion!.Value)
                              : null
                      };
                  })
                  .ToList();

                // Clamp về 0–100 cho chắc
                if (totalPercentModule < 0)
                {
                    totalPercentModule = 0;
                }
                else if (totalPercentModule > 100)
                {
                    totalPercentModule = 100;
                }
                unitResult.SkillScores = aggregatedSkillScores;
                unitResult.CorrectCount = totalCorrectCount;
                unitResult.CorrectTotal = totalCorrectTotal;
                unitResult.Percent = totalPercentModule;
            }

            unitResult.Status = EnumResultStatus.Done;
            await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.CorrectTotal, entity.Percent, entity.SkillScoresStr, entity.Status };
            });
            await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
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
            await UpdateUnitResultAsync(unitResult, unitModules, cancellationToken);
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
