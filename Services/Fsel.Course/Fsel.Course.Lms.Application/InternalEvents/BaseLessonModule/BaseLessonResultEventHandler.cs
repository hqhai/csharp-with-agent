// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseLessonModule
{
    using System.Threading;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
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

        public BaseLessonResultEventHandler(ILessonResultRepository lessonResultRepository,
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

        public async Task UpdateLessonResultAsync(LessonResult lessonResult, IList<LessonModule> lessonModules, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);

            // Kiểm tra các loại kết quả liên quan đến bài học
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

            bool hasUnfinished = videoResults.Any(x => x.Status != EnumResultStatus.Done)
                              || documentResults.Any(x => x.Status != EnumResultStatus.Done)
                              || classForumResults.Any(x => x.ResultStatus != EnumResultStatus.Done)
                              || homeWorkResults.Any(x => x.Status != EnumResultStatus.Done);
            if (hasUnfinished)
            {
                return;
            }

            var totalPercentModule = videoResults.Sum(x => x.PercentModule)
                                     + classForumResults.Sum(x => x.PercentModule)
                                     + documentResults.Sum(x => x.PercentModule)
                                     + homeWorkResults.Sum(x => x.PercentModule);

            var totalCorrectCount = videoResults.Sum(x => x.CorrectCount) +
                                        classForumResults.Sum(x => x.CorrectCount) +
                                        homeWorkResults.Sum(x => x.CorrectCount);

            var totalCorrectTotal =
                videoResults.Sum(x => x.CorrectTotal) +
                videoResults.Sum(x => x.CorrectTotal) +
                classForumResults.Sum(x => x.CorrectTotal) +
                homeWorkResults.Sum(x => x.CorrectTotal);

            var allSkillScores = Enumerable.Empty<SkillScores>()
             .Concat(videoResults.Where(x => x.VideoSkillScores != null).SelectMany(x => x.VideoSkillScores!)
                                 .SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()))
             .Concat(classForumResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()))
             .Concat(homeWorkResults.SelectMany(x => x.SkillScores ?? Enumerable.Empty<SkillScores>()));

            var aggregatedSkillScores = allSkillScores
              .GroupBy(s => new { s.SkillId, s.Skill })
              .Select(g =>
              {
                  // Lấy 1 mẫu để reuse name, v.v.
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
                      // Tuỳ nghiệp vụ: tổng hoặc trung bình CorrectQuestion
                      CorrectQuestion = g.Any(x => x.CorrectQuestion.HasValue)
                          ? g.Where(x => x.CorrectQuestion.HasValue).Sum(x => x.CorrectQuestion!.Value)
                          : (double?)null
                      // Percent KHÔNG cần set, getter sẽ tự tính từ CorrectCount / TotalCount
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
            lessonResult.SkillScores = aggregatedSkillScores;
            lessonResult.CorrectCount = totalCorrectCount;
            lessonResult.CorrectTotal = totalCorrectTotal;
            lessonResult.Percent = totalPercentModule;

            lessonResult.Status = EnumResultStatus.Done;
            await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
            {
                bulk.ColumnInputExpression = entity => new { entity.CorrectCount, entity.CorrectTotal, entity.Percent, entity.SkillScoresStr, entity.Status };
            });
            await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateLessonResultAsync(LessonResult lessonResult, Guid lessonModuleId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            var lessonModules = await GetLessonModulesAsync(lessonResult.LessonId);
            var currentModule = FindCurrentModule(lessonModules, lessonModuleId);
            if (currentModule == null)
            {
                return;
            }

            var nextModule = GetNextModule(lessonModules, currentModule);
            if (nextModule != null)
            {
                await UpdateNewResultLessonModule(nextModule, lessonResult, cancellationToken);

                var nextDone = await IsLessonModuleCompletedAsync(nextModule, lessonResult, cancellationToken);
                if (nextDone)
                {
                    await UpdateLessonResultAsync(lessonResult, lessonModules, cancellationToken);
                }
                return;
            }
            await UpdateLessonResultAsync(lessonResult, lessonModules, cancellationToken);
        }

        private async Task<bool> IsLessonModuleCompletedAsync(
        LessonModule module,
        LessonResult lessonResult,
        CancellationToken cancellationToken)
        {
            var resultId = lessonResult.Id;

            switch (module.LessonConfigType)
            {
                case EnumLessonConfigType.Video:
                    return !await _videoResultRepository.ReadQueryable
                        .AnyAsync(x => x.LessonResultId == resultId && x.Status != EnumResultStatus.Done, cancellationToken);

                case EnumLessonConfigType.Document:
                    return !await _documentResultRepository.ReadQueryable
                        .AnyAsync(x => x.LessonResultId == resultId && x.Status != EnumResultStatus.Done, cancellationToken);

                case EnumLessonConfigType.ClassForum:
                    return !await _classForumResultRepository.ReadQueryable
                        .AnyAsync(x => x.LessonResultId == resultId && x.ResultStatus != EnumResultStatus.Done, cancellationToken);

                case EnumLessonConfigType.HomeWork:
                    return !await _homeWorkResultRepository.ReadQueryable
                        .AnyAsync(x => x.LessonResultId == resultId && x.Status != EnumResultStatus.Done, cancellationToken);

                default:
                    return true;
            }
        }

        private static LessonModule? FindCurrentModule(IList<LessonModule> lessonModules, Guid? lessonModuleId)
        {
            if (!lessonModuleId.HasValue)
            {
                return null;
            }
            return lessonModules.FirstOrDefault(m => m.Id == lessonModuleId);
        }

        private static LessonModule? GetNextModule(IList<LessonModule> lessonModules, LessonModule currentModule)
        {
            return lessonModules.Where(m => m.OpenOrder > currentModule.OpenOrder)
                                .OrderBy(m => m.OpenOrder)
                                .FirstOrDefault();
        }

        private async Task UpdateNewResultLessonModule(LessonModule nextModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(nextModule);
            var initializer = _lessonItemInitializerFactory.Get(nextModule.LessonConfigType);
            if (initializer != null)
            {
                await initializer.InitializeAsync(nextModule, lessonResult, cancellationToken);
            }
            return;
        }

        private async Task<IList<LessonModule>> GetLessonModulesAsync(Guid id)
        {
            return await _lessonModuleCachingService.GetOrSetAsync(id.ToString(), async (ctx, _) =>
            {
                var lessonModules = await _lessonModuleRepository.ReadQueryable
                                                .Where(x => x.LessonId == id)
                                                .ToListAsync(_);

                return lessonModules.OrderBy(x => x.DisplayOrder).ToList();
            });
        }
    }
}
