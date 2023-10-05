// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Helpers;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalLessonResultEventHandler : BaseInternalEventHandler
    {
        public BaseInternalLessonResultEventHandler(IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        private const int TotalClassForum = 36;
        private const int PercentOccupyHomeWork = 30;
        private const int PercentOccupyVideo = 40;
        private const int PercentOccupyClassForum = 40;

        public async Task GetLessonResult(LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResult);
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
            List<SkillScores> groupedSkillScores = skillScores
                                    .GroupBy(x => x.Skill)
                                    .Select(group => new SkillScores
                                    {
                                        Skill = group.Key,
                                        Scores = group.Average(x => x.Scores),
                                        TotalCount = group.Sum(x => x.TotalCount),
                                        CorrectCount = group.Sum(x => x.CorrectCount),
                                        CountQuestion = group.Sum(x => x.CountQuestion),
                                        TotalQuestion = group.Sum(x => x.TotalQuestion),
                                        Percent = NumberHelper.ConvertDouble(group.Average(x => x.Percent)),
                                    }).ToList();
            lessonResult.CorrectCount = (int)(correctVideo + correctClassForum + correctHomeWork ?? default);
            lessonResult.CorrectTotal = (int)(totalVideo + totalHomeWork + totalClassForum ?? default);
            lessonResult.Percent = NumberHelper.ConvertDoublePercent(percentVideo * PercentOccupyVideo + percentHomeWork * PercentOccupyHomeWork + percentClassForum * PercentOccupyClassForum);
            lessonResult.SkillScores = groupedSkillScores;
        }

        private async Task<(double?, double?, double, IList<SkillScores>?)> GetVideoResult(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            if (videoResult != null)
            {
                var skillScores = videoResult.VideoSkillScores?.FirstOrDefault(x => x.Type == EnumTimeCodeType.Standalone)?.SkillScores;
                if (skillScores != null && skillScores.Count > 0)
                {
                    return (skillScores.Sum(x => x.TotalCount), skillScores.Sum(x => x.TotalCount), NumberHelper.ConvertDouble(videoResult.Percent), skillScores);
                }
            }

            return (null, null, default, null);
        }

        private async Task<(double?, double?, double, IList<SkillScores>?)> GetClassForumResult(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var classForumResult = await _classForumResultRepository.Queryable.Include(x => x.ClassForumScores)
                                                                            .Include(x => x.ClassForum)
                                                                            .FirstOrDefaultAsync(x => x.LessonResultId == lessonResultId, cancellationToken);
            if (classForumResult != null)
            {
                var skillScores = new SkillScores
                {
                    CorrectCount = classForumResult.ClassForumScores?.Sum(x => x.Score) ?? default,
                    CountQuestion = 1,
                    TotalQuestion = 1,
                    TotalCount = TotalClassForum,
                    Skill = classForumResult.ClassForum?.CourseSkill ?? default,
                };
                skillScores.Percent = NumberHelper.ConvertPercentDouble((double)skillScores.CorrectCount / skillScores.TotalCount);
                return (skillScores.CorrectCount, skillScores.TotalCount, skillScores.Percent, new List<SkillScores> { skillScores });
            }

            return (null, null, default, null);
        }

        private async Task<(double?, double?, double, IList<SkillScores>?)> GetHomeResults(Guid lessonResultId, CancellationToken cancellationToken)
        {
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == lessonResultId && x.Status == EnumResultStatus.Done).ToListAsync(cancellationToken);
            if (!homeWorkResults.Any())
            {
                return (null, null, default, null);
            }
            var skillScores = homeWorkResults.Where(x => x.SkillScores != null && x.SkillScores.Any()).SelectMany(x => x.SkillScores!).ToList();
            skillScores.ForEach(x => x.Percent = x.TotalCount > 0 ? NumberHelper.ConvertPercentDouble((double)x.CorrectCount / x.TotalCount) : default);
            return (skillScores.Sum(x => x.TotalCount), skillScores.Sum(x => x.TotalCount), NumberHelper.ConvertDoubleDecimal(skillScores.Average(x => x.Scores)), skillScores);
        }
    }
}
