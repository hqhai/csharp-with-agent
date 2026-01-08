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
    using Microsoft.AspNetCore.Cors.Infrastructure;
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
            var videoResults = await _videoResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

            var documentResults = await _documentResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

            var classForumResults = await _classForumResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

            var homeWorkResults = await _homeWorkResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == lessonResult.Id)
                .ToListAsync(cancellationToken);

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
                        SkillFilePath = first.SkillFilePath,
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
            if (lessonResult.Status == EnumResultStatus.Done)
            {
                lessonResult.CompletionDate = DateTime.UtcNow;
            }
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
                        entity.Status,
                        entity.CompletionDate
                    };
                });

            await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken)
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
                currentModule.Id,
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
         Guid currentModuleId,
         CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            lessonModules ??= Array.Empty<LessonModule>();

            if (lessonModules.Count == 0)
            {
                return true;
            }
            var resultId = lessonResult.Id;

            // Tập module cần hoàn thành
            var requiredModuleIds = lessonModules
                .Select(m => m.Id) // hoặc m.OriginalId tuỳ bạn đang dùng gì để map
                .ToHashSet();

            // Query các module đã done (lọc LessonModuleId != null)
            var videoDone = _videoResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == resultId
                            && x.Status == EnumResultStatus.Done
                            && x.LessonModuleId != null)
                .Select(x => x.LessonModuleId!.Value);

            var docDone = _documentResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == resultId
                            && x.Status == EnumResultStatus.Done)
                .Select(x => x.LessonModuleId);

            var forumDone = _classForumResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == resultId
                            && x.ResultStatus == EnumResultStatus.Done
                            && x.LessonModuleId != null)
                .Select(x => x.LessonModuleId!.Value);

            var hwDone = _homeWorkResultRepository.Queryable.AsNoTracking()
                .Where(x => x.LessonResultId == resultId
                            && x.Status == EnumResultStatus.Done
                            && x.LessonModuleId != null)
                .Select(x => x.LessonModuleId!.Value);

            var doneModuleIds = await videoDone
                .Union(docDone)
                .Union(forumDone)
                .Union(hwDone)
                .ToListAsync(cancellationToken);

            // Chỉ tính những cái thuộc required (tránh tính module “rác”/ngoài danh sách)
            var doneModule = doneModuleIds.Where(id => requiredModuleIds.Contains(id));
            var doneRequiredCount = doneModule.Count();
            if (doneRequiredCount == requiredModuleIds.Count)
            {
                return true;
            }
            return doneModule.Where(x => x != currentModuleId).Count() == requiredModuleIds.Where(x => x != currentModuleId).Count();
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
            var minOrder = lessonModules.Where(m => m.OpenOrder > currentModule.OpenOrder).OrderBy(m => m.OpenOrder).FirstOrDefault();
            if (minOrder == null)
            {
                return new List<LessonModule>();
            }
            return lessonModules.Where(m => m.OpenOrder == minOrder.OpenOrder)
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
