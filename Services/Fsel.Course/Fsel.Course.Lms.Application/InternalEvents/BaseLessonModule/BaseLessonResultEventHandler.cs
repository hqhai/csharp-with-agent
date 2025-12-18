// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseLessonModule
{
    using System.Linq;
    using System.Threading;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
    using Fsel.Shared.Constants;
    using Microsoft.EntityFrameworkCore;

    public class BaseLessonResultEventHandler
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly ILessonModuleCachingService _lessonModuleCachingService;
        private readonly IClassForumResultRepository _classForumResultRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IDocumentResultRepository _documentResultRepository;
        private readonly ILessonItemInitializerFactory _lessonItemInitializerFactory;

        public BaseLessonResultEventHandler(
            ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IVideoResultRepository videoResultRepository,
            IDocumentResultRepository documentResultRepository,
            ILessonItemInitializerFactory lessonItemInitializerFactory)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _lessonModuleCachingService = lessonModuleCachingService;
            _classForumResultRepository = classForumResultRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
            _videoResultRepository = videoResultRepository;
            _documentResultRepository = documentResultRepository;
            _lessonItemInitializerFactory = lessonItemInitializerFactory;
        }

        /// <summary>
        /// Tính lại điểm, skill, percent và set LessonResult = Done nếu tất cả result đã Done.
        /// </summary>
        public async Task UpdateLessonResultAsync(
            LessonResult lessonResult,
            IList<LessonModule> lessonModules,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModules);
            ArgumentNullException.ThrowIfNull(lessonResult);

            // Lấy tất cả result của lesson
            var videoResults = await _videoResultRepository.ReadQueryable
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

            var documentResults = await _documentResultRepository.ReadQueryable
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

            var classForumResults = await _classForumResultRepository.ReadQueryable
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

            var homeWorkResults = await _homeWorkResultRepository.ReadQueryable
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

            var hasUnfinished = videoResults.Any(x => x.Status != EnumResultStatus.Done)
                                || documentResults.Any(x => x.Status != EnumResultStatus.Done)
                                || classForumResults.Any(x => x.ResultStatus != EnumResultStatus.Done)
                                || homeWorkResults.Any(x => x.Status != EnumResultStatus.Done);

            if (hasUnfinished)
            {
                return;
            }
            // Tất cả result đều Done => tính điểm tổng
            var totalPercentModule =
                  videoResults.Sum(x => x.PercentModule)
                + classForumResults.Sum(x => x.PercentModule)
                + documentResults.Sum(x => x.PercentModule)
                + homeWorkResults.Sum(x => x.PercentModule);

            var totalCorrectCount =
                  videoResults.Sum(x => x.CorrectCount)
                + classForumResults.Sum(x => x.CorrectCount)
                + homeWorkResults.Sum(x => x.CorrectCount);
            // Nếu Document có CorrectCount thì thêm:
            // + documentResults.Sum(x => x.CorrectCount);

            var totalCorrectTotal =
                  videoResults.Sum(x => x.CorrectTotal)
                + classForumResults.Sum(x => x.CorrectTotal)
                + homeWorkResults.Sum(x => x.CorrectTotal);

            var allSkillScores = Enumerable.Empty<SkillScores>()
                .Concat(videoResults
                    .Where(x => x.VideoSkillScores != null)
                    .SelectMany(x => x.VideoSkillScores!)
                    .SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()))
                .Concat(classForumResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()))
                .Concat(homeWorkResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()));

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
                        Scores = g.Sum(x => x.Scores),
                        TotalCount = g.Sum(x => x.TotalCount),
                        CorrectCount = g.Sum(x => x.CorrectCount),
                        TotalQuestion = g.Sum(x => x.TotalQuestion),
                        CountQuestion = g.Sum(x => x.CountQuestion),
                        TokenReceived = g.Sum(x => x.TokenReceived),
                        CorrectQuestion = g.Any(x => x.CorrectQuestion.HasValue)
                            ? g.Where(x => x.CorrectQuestion.HasValue).Sum(x => x.CorrectQuestion!.Value) : null
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

            lessonResult.SkillScores = aggregatedSkillScores;
            lessonResult.CorrectCount = totalCorrectCount;
            lessonResult.CorrectTotal = totalCorrectTotal;
            lessonResult.Percent = totalPercentModule;
            lessonResult.Status = EnumResultStatus.Done;

            await _lessonResultRepository.BulkUpdateList(
                new[] { lessonResult },
                bulk =>
                {
                    bulk.ColumnInputExpression = entity => new
                    {
                        entity.CorrectCount,
                        entity.CorrectTotal,
                        entity.Percent,
                        entity.SkillScoresStr,
                        entity.Status
                    };
                });

            await _lessonResultRepository.UnitOfWork
                .SaveEntitiesAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Được gọi khi 1 LessonModule hoàn thành.
        /// - Mở tất cả module phía sau.
        /// - Nếu số module có Result Done == số module cần làm → Done lesson.
        /// </summary>
        public async Task UpdateLessonResultAsync(
            LessonResult lessonResult,
            Guid lessonModuleId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);

            var lessonModules = await GetLessonModulesAsync(lessonResult.LessonId);
            var currentModule = FindCurrentModule(lessonModules, lessonModuleId);
            if (currentModule == null)
            {
                return;
            }

            var nextModules = GetNextModules(lessonModules, currentModule);
            if (nextModules.Any())
            {
                foreach (var nextModule in nextModules)
                {
                    await UpdateNewResultLessonModule(nextModule, lessonResult, cancellationToken);
                }
            }

            // Check cả lesson đã đủ Result Done chưa (ResultDoneCount == LessonModuleCount)
            var lessonDone = await IsLessonModulesCompletedAsync(
                lessonResult,
                lessonModules,
                cancellationToken);

            if (lessonDone)
            {
                await UpdateLessonResultAsync(lessonResult, lessonModules, cancellationToken);
            }
        }

        /// <summary>
        /// Lesson được coi là hoàn thành khi:
        /// SỐ MODULE CẦN LÀM == SỐ MODULE ĐÃ CÓ RESULT Ở TRẠNG THÁI DONE.
        /// </summary>
        private async Task<bool> IsLessonModulesCompletedAsync(
        LessonResult lessonResult,
        IList<LessonModule> lessonModules,
        CancellationToken cancellationToken)
        {
            var resultId = lessonResult.Id;

            var queryVideo = _videoResultRepository.ReadQueryable
                    .Where(x => x.LessonResultId == resultId && x.Status == EnumResultStatus.Done)
                    .Select(x => x.Id);
            var queryDocument = _documentResultRepository.ReadQueryable
                    .Where(x => x.LessonResultId == resultId && x.Status == EnumResultStatus.Done)
                    .Select(x => x.Id);

            var queryClassForum = _classForumResultRepository.ReadQueryable
                    .Where(x => x.LessonResultId == resultId && x.ResultStatus == EnumResultStatus.Done)
                    .Select(x => x.Id);

            var queryHomeWork = _homeWorkResultRepository.ReadQueryable
                    .Where(x => x.LessonResultId == resultId && x.Status == EnumResultStatus.Done)
                    .Select(x => x.Id);

            var doneModuleIds = await queryVideo.Union(queryHomeWork).Union(queryClassForum).Union(queryDocument).ToListAsync(cancellationToken);
            return doneModuleIds.Count == lessonModules.Count;
        }

        private static LessonModule? FindCurrentModule(
            IList<LessonModule> lessonModules,
            Guid? lessonModuleId)
        {
            if (!lessonModuleId.HasValue)
            {
                return null;
            }

            return lessonModules.FirstOrDefault(m => m.Id == lessonModuleId);
        }

        private static IList<LessonModule> GetNextModules(
            IList<LessonModule> lessonModules,
            LessonModule currentModule)
        {
            return lessonModules
                .Where(m => m.OpenOrder == currentModule.OpenOrder + 1)
                .OrderBy(m => m.OpenOrder)
                .ToList();
        }

        private async Task UpdateNewResultLessonModule(
            LessonModule nextModule,
            LessonResult lessonResult,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);

            var initializer = _lessonItemInitializerFactory.Get(nextModule.LessonConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, lessonResult, cancellationToken);
            }
        }

        private async Task<IList<LessonModule>> GetLessonModulesAsync(Guid lessonId)
        {
            return await _lessonModuleCachingService.GetOrSetAsync(lessonId.ToString(), async (ctx, _) =>
            {
                var lessonModules = await _lessonModuleRepository.ReadQueryable
                    .Where(x => x.LessonId == lessonId)
                    .ToListAsync(_);

                return lessonModules
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();
            });
        }
    }
}
