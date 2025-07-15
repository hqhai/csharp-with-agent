// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Commands.LessonCmd.V1i1;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class BaseInternalLessonResultEventHandler : BaseInternalEventHandler
    {
        private const int PercentOccupyHomeWork = 30;
        private const int PercentOccupyVideo = 40;
        private const int PercentOccupyClassForum = 30;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILogger<BaseInternalLessonResultEventHandler> _logger;

        public BaseInternalLessonResultEventHandler(ISystemService systemService,
            AppSetting appSetting,
            ICourseUnitMockTestRepository courseUnitMockTestRepository,
            IMediator mediator,
            IUserService userService,
            ILogger<BaseInternalLessonResultEventHandler> logger,
            ILessonResultRepository lessonResultRepository,
            SaveUserCourseSettingPublisher saveUserCourseSettingPublisher,
            IVideoResultRepository videoResultRepository,
            IClassForumResultRepository classForumResultRepository,
            IUnitResultRepository unitResultRepository,
            ICourseResultRepository courseResultRepository,
            ICourseRepository courseRepository,
            IUnitRepository unitRepository,
            IFinalTestResultRepository finalTestResultRepository,
            IMockTestResultRepository mockTestResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            QuestBoardPublisher questBoardPublisher,
            IOrderService orderService,
            ILessonNoteRepository lessonNoteRepository, NotificationMessagePublisher notificationMessagePublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, logger, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, orderService, lessonNoteRepository, lessonResultRepository, notificationMessagePublisher)
        {
            _lessonResultRepository = lessonResultRepository;
            _logger = logger;
        }

        public async Task UpdateLessonResultAsync(LessonResult? lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            try
            {
                var isHomeWorksDone = lessonResult.HomeWorkResults.Any() && lessonResult.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done);
                var isClassForumDone = lessonResult.ClassForumResults.Any() && lessonResult.ClassForumResults.Any(x => x.Status != EnumClassForumResultStatus.Draft);
                if (isClassForumDone && isHomeWorksDone && lessonResult.Status != EnumResultStatus.Done)
                {
                    // var courseId = lessonResult.CourseId;
                    // var userId = lessonResult.CreatedUserId;
                    // await DoQuestBoard(userId, courseId, cancellationToken);

                    lessonResult.Status = EnumResultStatus.Done;
                    await UpdateAsync(lessonResult, cancellationToken).ConfigureAwait(false);
                    await _mediator.Send(new CreateLuckyTicketCommand() { LessonResultId = lessonResult.Id }, cancellationToken).ConfigureAwait(false);
                }
                else if (lessonResult.Status == EnumResultStatus.Done)
                {
                    await UpdateAsync(lessonResult, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger LessonResult : {ex.Message} ");
            }
        }

        private async Task UpdateAsync(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            var baseScoreResult = await GetLessonResult(lessonResult, cancellationToken);

            try
            {
                lessonResult.CorrectCount = (int)baseScoreResult.CorrectCount;
                lessonResult.CorrectTotal = (int)baseScoreResult.CorrectTotal;
                lessonResult.Percent = baseScoreResult.Percent;
                lessonResult.SkillScores = baseScoreResult.SkillScores;
                await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult }, bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId };
                });
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger Save LessonResult : {ex.Message} ");
            }
        }

        private async Task<BaseScoreResultModule> GetLessonResult(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            var baseScoreResultVideo = await GetScoreResultOfVideoResultAsync(lessonResult.Id, cancellationToken);
            var baseScoreResultHomeWork = await GetScoreResultOfHomeResultsAsync(lessonResult.Id, cancellationToken);
            var baseScoreResultClassForum = await GetScoreResultOfClassForumResultAsync(lessonResult.Id, cancellationToken);
            var baseScoreResultModules = new List<BaseScoreResultModule> { baseScoreResultVideo, baseScoreResultHomeWork, baseScoreResultClassForum };
            List<SkillScores> groupedSkillScores = baseScoreResultModules.Where(x => x.SkillScores != null && x.SkillScores.Any())
                                                                        .SelectMany(x => x.SkillScores!)
                                                                        .GroupBy(x => x.Skill)
                                                                        .Select(group => GetSumSkillScore(group))
                                                                        .OrderBy(x => x.Skill).ToList();

            return new BaseScoreResultModule
            {
                CorrectCount = baseScoreResultModules.Sum(x => x.CorrectCount),
                CorrectTotal = baseScoreResultModules.Sum(x => x.CorrectTotal),
                Percent = baseScoreResultModules.Sum(x => x.Percent),
                SkillScores = groupedSkillScores
            };
        }

        private async Task<BaseScoreResultModule> GetScoreResultOfVideoResultAsync(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            if (videoResult != null)
            {
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                return GetValueAsync(skillScores, PercentOccupyVideo);
            }
            return GetValueAsync(default);
        }

        private async Task<BaseScoreResultModule> GetScoreResultOfClassForumResultAsync(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForum)
                                                                            .FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            return GetValueAsync(classForumResult?.SkillScores, PercentOccupyClassForum);
        }

        private async Task<BaseScoreResultModule> GetScoreResultOfHomeResultsAsync(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == lessonResultId && x.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);
            if (!homeWorkResults.Any())
            {
                return GetValueAsync(default);
            }
            var skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSumSkillScore(x)).ToList();
            return GetValueAsync(skillScores, PercentOccupyHomeWork);
        }

        private static BaseScoreResultModule GetValueAsync(IList<SkillScores>? skillScores, double percentAchieved = default)
        {
            if (skillScores != null && skillScores.Any())
            {
                var correctCount = skillScores.Sum(x => x.CorrectCount);
                var correctTotal = skillScores.Sum(x => x.TotalCount);
                return new BaseScoreResultModule
                {
                    CorrectCount = correctCount,
                    CorrectTotal = correctTotal,
                    Percent = correctTotal > 0 ? NumberHelper.ConvertRound((correctCount / correctTotal) * percentAchieved) : default,
                    SkillScores = skillScores
                };
            }
            else
            {
                return new BaseScoreResultModule();
            }
        }

        private class BaseScoreResultModule
        {
            public double CorrectCount { get; set; }
            public double CorrectTotal { get; set; }
            public double Percent { get; set; }
            public IList<SkillScores>? SkillScores { get; set; }
        }
    }
}
