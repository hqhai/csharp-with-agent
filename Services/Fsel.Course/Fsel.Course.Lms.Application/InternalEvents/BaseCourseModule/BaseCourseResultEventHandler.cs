// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseCourseModule
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
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
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;

        public BaseCourseResultEventHandler(ICourseModuleCachingService courseModuleCachingService,
            ICourseModuleRepository courseModuleRepository,
            ICourseResultRepository courseResultRepository,
            ICourseItemInitializerFactory courseItemInitializerFactory,
            ITestGroupResultRepository testGroupResultRepository,
            IUnitResultRepository unitResultRepository)
        {
            _courseModuleCachingService = courseModuleCachingService;
            _courseModuleRepository = courseModuleRepository;
            _courseResultRepository = courseResultRepository;
            _courseItemInitializerFactory = courseItemInitializerFactory;
            _testGroupResultRepository = testGroupResultRepository;
            _unitResultRepository = unitResultRepository;
        }

        public async Task UpdateCourseResultAsync(CourseResult courseResult, IList<CourseModule> courseModules, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(courseResult);
            // Tính toán lại tiến độ hoàn thành bài học
            var testResults = await _testGroupResultRepository.ReadQueryable
                                                                         .Where(x => x.CourseResultId == courseResult.Id && x.Status == EnumResultStatus.Done)
                                                                         .Where(x => !x.UnitModuleId.HasValue)
                                                                         .SelectMany(x => x.TestResults)
                                                                         .ToListAsync(cancellationToken);

            var unitResults = await _unitResultRepository.ReadQueryable
                                                                 .Where(x => x.CourseResultId == courseResult.Id && x.Status == EnumResultStatus.Done)
                                                                 .ToListAsync(cancellationToken);
            var totalPercentModule = unitResults.Sum(x => x.PercentModule) + testResults.Sum(x => x.PercentModule);
            var totalCorrectCount = unitResults.Sum(x => x.CorrectCount) + testResults.Sum(x => x.CorrectCount);
            var totalCorrectTotal = unitResults.Sum(x => x.CorrectTotal) + testResults.Sum(x => x.CorrectTotal);

            var allSkillScores = Enumerable.Empty<SkillScores>()
             .Concat(unitResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()))
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
            courseResult.SkillScores = aggregatedSkillScores;
            courseResult.CorrectCount = totalCorrectCount;
            courseResult.CorrectTotal = totalCorrectTotal;
            courseResult.Percent = totalPercentModule;

            courseResult.Status = EnumResultStatus.Done;
            await _courseResultRepository.BulkUpdateList(new List<CourseResult> { courseResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.CorrectTotal, entity.Percent, entity.SkillScoresStr, entity.Status };
            });
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
            await UpdateCourseResultAsync(courseResult, courseModules, cancellationToken);
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
