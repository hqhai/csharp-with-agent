// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalLessonResultEventHandler : BaseInternalEventHandler
    {
        private const int TotalClassForum = 36;
        private const int PercentOccupyHomeWork = 30;
        private const int PercentOccupyVideo = 40;
        private const int PercentOccupyClassForum = 40;
        private readonly FinishOneLessonPublisher _finishOneLessonPublisher;

        public BaseInternalLessonResultEventHandler(ISystemService systemService, AppSetting appSetting, FinishOneLevelPassPublisher finishOneLevelPassPublisher, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(systemService, appSetting, finishOneLevelPassPublisher, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task UpdateLessonResultAsync(LessonResult? lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            var isHomeWorksDone = lessonResult.HomeWorkResults.All(x => x.Status == EnumResultStatus.Done);
            var isClassForumDone = lessonResult.ClassForumResults.Any(x => (x.Status == EnumClassForumResultStatus.PendingForGrading || x.Status == EnumClassForumResultStatus.Graded));
            if (isClassForumDone && isHomeWorksDone && lessonResult.Status != EnumResultStatus.Done)
            {
                await _finishOneLessonPublisher.Publish(lessonResult, cancellationToken).ConfigureAwait(false);
                await UpdateLessonResult(lessonResult, cancellationToken);
                lessonResult.Status = EnumResultStatus.Done;
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _lessonResultRepository.Update(lessonResult);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task UpdateLessonResult(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
            var (correctCount, correctTotal, percent, skillScores) = await GetLessonResult(lessonResult, cancellationToken);
            lessonResult.CorrectCount = correctCount;
            lessonResult.CorrectTotal = correctTotal;
            lessonResult.Percent = percent;
            lessonResult.SkillScores = skillScores;
        }

        private async Task<(int, int, double, IList<SkillScores>?)> GetLessonResult(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            var (correctVideo, totalVideo, percentVideo, skillScoreVideos) = await GetVideoResult(lessonResult.Id, cancellationToken);
            var (correctHomeWork, totalHomeWork, percentHomeWork, skillScoreHomeWorks) = await GetHomeResults(lessonResult.Id, cancellationToken);
            var (correctClassForum, totalClassForum, percentClassForum, skillScoreClassForums) = await GetClassForumResult(lessonResult.Id, cancellationToken);
            var skillScores = new List<SkillScores>();
            if (skillScoreVideos != null && skillScoreVideos.Any())
            {
                skillScores = skillScores.Union(skillScoreVideos).ToList();
            }
            if (skillScoreHomeWorks != null && skillScoreHomeWorks.Any())
            {
                skillScores = skillScores.Union(skillScoreHomeWorks).ToList();
            }
            if (skillScoreClassForums != null && skillScoreClassForums.Any())
            {
                skillScores = skillScores.Union(skillScoreClassForums).ToList();
            }
            List<SkillScores> groupedSkillScores = skillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
            var correctCounts = new List<double> { correctVideo, correctHomeWork, correctClassForum };
            var correctTotals = new List<double> { totalVideo, totalHomeWork, totalClassForum };
            var percents = new List<double> { percentVideo * PercentOccupyVideo, percentHomeWork * PercentOccupyHomeWork, percentClassForum * PercentOccupyClassForum };
            return ((int)correctCounts.Sum(), (int)correctTotals.Sum(), NumberHelper.ConvertDoublePercent(percents.Sum()), groupedSkillScores);
        }

        private async Task<(double, double, double, IList<SkillScores>?)> GetVideoResult(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            if (videoResult != null)
            {
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                return GetValueAsync(skillScores);
            }
            return GetValueAsync(default);
        }

        private async Task<(double, double, double, IList<SkillScores>?)> GetClassForumResult(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumScores)
                                                                            .Include(x => x.ClassForum)
                                                                            .FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            if (classForumResult != null)
            {
                var skillScores = new List<SkillScores> { GetSkillScores(classForumResult) };
                return GetValueAsync(skillScores);
            }

            return GetValueAsync(default);
        }

        private async Task<(double, double, double, IList<SkillScores>?)> GetHomeResults(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == lessonResultId && x.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);
            if (!homeWorkResults.Any())
            {
                return GetValueAsync(default);
            }
            var skillScores = homeWorkResults.SelectMany(x => x.SkillScores!).GroupBy(x => x.Skill).Select(x => GetSumSkillScore(x)).ToList();
            skillScores.ForEach(x => x.Percent = x.TotalCount > 0 ? NumberHelper.ConvertPercentDouble((double)x.CorrectCount / x.TotalCount) : default);
            return GetValueAsync(skillScores);
        }

        private static (double, double, double, IList<SkillScores>?) GetValueAsync(IList<SkillScores>? skillScores)
        {
            if (skillScores != null && skillScores.Any())
            {
                return (skillScores.Sum(x => x.CorrectCount), skillScores.Sum(x => x.TotalCount), NumberHelper.ConvertDoubleDecimal(skillScores.Average(x => x.Percent)), skillScores);
            }
            else
            {
                return (default, default, default, null);
            }
        }
    }
}