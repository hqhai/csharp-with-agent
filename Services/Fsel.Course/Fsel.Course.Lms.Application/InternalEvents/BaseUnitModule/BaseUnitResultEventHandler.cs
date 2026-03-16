// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices;
    using Fsel.Shared.Constants;
    using Microsoft.AspNetCore.Cors.Infrastructure;
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
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly IUnitItemInitializerFactory _unitItemInitializerFactory;
        private readonly SendMailCompleteUnitPublisher _sendMailCompleteUnitPublisher;

        public BaseUnitResultEventHandler(
            IUnitModuleCachingService unitModuleCachingService,
            IUnitModuleRepository unitModuleRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ITestGroupResultRepository testGroupResultRepository,
            IUnitItemInitializerFactory unitItemInitializerFactory,
            SendMailCompleteUnitPublisher sendMailCompleteUnitPublisher)
        {
            _unitModuleCachingService = unitModuleCachingService;
            _unitModuleRepository = unitModuleRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _unitItemInitializerFactory = unitItemInitializerFactory;
            _sendMailCompleteUnitPublisher = sendMailCompleteUnitPublisher;
        }

        /// <summary>
        /// Tính lại điểm / skill / % và set UnitResult = Done
        /// CHỈ khi tất cả LessonResult + TestGroupResult + TestResult đã Done.
        /// </summary>
        public async Task UpdateUnitResultAsync(
            UnitResult unitResult,
            IList<UnitModule> unitModules,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unitResult);

            // Nếu không có module nào thì coi như 100% và Done luôn
            if (!unitModules.Any())
            {
                unitResult.Percent = ValueSettings.PercentMaxValue;
                if (unitResult.Status == EnumResultStatus.Done)
                {
                    unitResult.CompletionDate = DateTime.UtcNow;
                }
                unitResult.Status = EnumResultStatus.Done;

                await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new
                    {
                        entity.Percent,
                        entity.Status,
                        entity.CompletionDate
                    };
                });

                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

                await _sendMailCompleteUnitPublisher.Publish(new SendMailCompleteUnitCommandModel() { UnitResultId = unitResult.Id }, cancellationToken);

                return;
            }

            // Lấy tất cả LessonResult / TestGroupResult / TestResult thuộc UnitResult này
            var lessonResultsAll = await _lessonResultRepository.Queryable.AsNoTracking()
                .Where(x => x.UnitResultId == unitResult.Id)
                .ToListAsync(cancellationToken);

            var testGroupResultsAll = await _testGroupResultRepository.Queryable.AsNoTracking()
                .Where(x => x.UnitResultId == unitResult.Id)
                .Include(x => x.TestResults)
                .ToListAsync(cancellationToken);

            var testResultsAll = testGroupResultsAll
                .SelectMany(x => x.TestResults ?? Enumerable.Empty<TestResult>())
                .ToList();

            // Đến đây: tất cả con đều Done → tính tổng hợp
            var lessonResults = lessonResultsAll
                .Where(x => x.Status == EnumResultStatus.Done)
                .ToList();

            var testResults = testResultsAll
                .Where(x => x.Status == EnumResultStatus.Done)
                .ToList();

            var totalPercentModule =
                  lessonResults.Sum(x => x.PercentModule)
                + testResults.Sum(x => x.PercentModule);

            var totalCorrectCount =
                  lessonResults.Sum(x => x.CorrectCount)
                + testResults.Sum(x => x.CorrectCount);

            var totalCorrectTotal =
                  lessonResults.Sum(x => x.CorrectTotal)
                + testResults.Sum(x => x.CorrectTotal);

            var allSkillScores = Enumerable.Empty<SkillScores>()
                .Concat(lessonResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()))
                .Concat(testResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()));

            var aggregatedSkillScores = allSkillScores
                .GroupBy(s => new { s.SkillId })
                .Select(g =>
                {
                    var first = g.First();

                    return new SkillScores
                    {
                        Skill = first.Skill,
                        SkillId = first.SkillId,
                        SkillName = first.SkillName,
                        SkillFilePath = first.SkillFilePath,
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

            // Clamp 0–100
            if (totalPercentModule < ValueSettings.PercentMinValue)
            {
                totalPercentModule = ValueSettings.PercentMinValue;
            }
            else if (totalPercentModule > ValueSettings.PercentMaxValue)
            {
                totalPercentModule = ValueSettings.PercentMaxValue;
            }

            unitResult.SkillScores = aggregatedSkillScores;
            unitResult.CorrectCount = totalCorrectCount;
            unitResult.CorrectTotal = totalCorrectTotal;
            unitResult.Percent = totalPercentModule;
            if (unitResult.Status != EnumResultStatus.Done)
            {
                unitResult.CompletionDate = DateTime.UtcNow;
            }
            unitResult.Status = EnumResultStatus.Done;

            await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new
                {
                    entity.CorrectCount,
                    entity.CorrectTotal,
                    entity.Percent,
                    entity.SkillScoresStr,
                    entity.Status,
                    entity.CompletionDate
                };
            });

            await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);

            await _sendMailCompleteUnitPublisher.Publish(new SendMailCompleteUnitCommandModel() { UnitResultId = unitResult.Id }, cancellationToken);
        }

        /// <summary>
        /// Được gọi khi 1 UnitModule trong Unit hoàn thành.
        /// - Nếu còn module phía sau -> mở TẤT CẢ các module phía sau.
        /// - Nếu không còn module phía sau -> thử Done UnitResult (nếu tất cả con đã Done).
        /// </summary>
        public async Task UpdateUnitResultAsync(
            UnitResult unitResult,
            Guid unitModuleId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unitResult);

            var unitModules = await GetUnitModulesAsync(unitResult.UnitId);
            var currentModule = FindCurrentModule(unitModules, unitModuleId);
            if (currentModule == null)
            {
                return;
            }

            var nextModules = GetNextModules(unitModules, currentModule);

            if (nextModules.Any())
            {
                // Mở tất cả module phía sau
                foreach (var module in nextModules)
                {
                    await UpdateNewResultUnitModule(module, unitResult, cancellationToken);
                }
            }

            // Check cả Unit đã đủ Result Done chưa (ResultDoneCount == LessonModuleCount)
            var unitDone = await IsUnitModulesCompletedAsync(
                unitResult,
                unitModules,
                currentModule.Id,
                cancellationToken);

            if (unitDone)
            {
                await UpdateUnitResultAsync(unitResult, unitModules, cancellationToken);
            }
        }

        private async Task<bool> IsUnitModulesCompletedAsync(
        UnitResult unitResult,
        IList<UnitModule> unitModules,
         Guid currentModuleId,
        CancellationToken cancellationToken)
        {
            var resultId = unitResult.Id;
            // Tập module cần hoàn thành
            var requiredModuleIds = unitModules
                .Select(m => m.Id) // hoặc m.OriginalId tuỳ bạn đang dùng gì để map
                .ToHashSet();

            var queryLesson = _lessonResultRepository.Queryable.AsNoTracking()
                    .Where(x => x.UnitResultId == resultId && x.Status == EnumResultStatus.Done
                            && x.UnitModuleId != null)
                    .Select(x => x.UnitModuleId!.Value);
            var queryTest = _testGroupResultRepository.Queryable.AsNoTracking()
                    .Where(x => x.UnitResultId == resultId && x.Status == EnumResultStatus.Done
                            && x.UnitModuleId != null)
                    .Select(x => x.UnitModuleId!.Value);

            var doneModuleIds = await queryLesson.Union(queryTest).ToListAsync(cancellationToken);

            // Chỉ tính những cái thuộc required (tránh tính module “rác”/ngoài danh sách)
            var doneModule = doneModuleIds.Where(id => requiredModuleIds.Contains(id));
            var doneRequiredCount = doneModule.Count();
            if (doneRequiredCount == requiredModuleIds.Count)
            {
                return true;
            }
            return doneModule.Where(x => x != currentModuleId).Count() == requiredModuleIds.Where(x => x != currentModuleId).Count();
        }

        private static UnitModule? FindCurrentModule(
            IList<UnitModule> unitModules,
            Guid? unitModuleId)
        {
            if (!unitModuleId.HasValue)
            {
                return null;
            }

            return unitModules.FirstOrDefault(m => m.Id == unitModuleId);
        }

        /// <summary>
        /// Lấy tất cả module phía sau current (OpenOrder > current.OpenOrder).
        /// </summary>
        private static IList<UnitModule> GetNextModules(
            IList<UnitModule> unitModules,
            UnitModule currentModule)
        {
            var minOrder = unitModules.Where(m => m.OpenOrder > currentModule.OpenOrder).OrderBy(m => m.OpenOrder).FirstOrDefault();
            if (minOrder == null)
            {
                return new List<UnitModule>();
            }
            return unitModules
                .Where(m => m.OpenOrder == minOrder.OpenOrder)
                .OrderBy(m => m.OpenOrder)
                .ToList();
        }

        private async Task UpdateNewResultUnitModule(
        UnitModule nextModule,
        UnitResult unitResult,
        CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);

            var initializer = _unitItemInitializerFactory.Get(nextModule.UnitConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, unitResult, cancellationToken);
            }
        }

        public async Task<IList<UnitModule>> GetUnitModulesAsync(Guid id)
        {
            return await _unitModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var unitModules = await _unitModuleRepository.ReadQueryable
                    .Where(x => x.UnitId == id)
                    .ToListAsync(_);

                return unitModules
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();
            });
        }
    }
}
