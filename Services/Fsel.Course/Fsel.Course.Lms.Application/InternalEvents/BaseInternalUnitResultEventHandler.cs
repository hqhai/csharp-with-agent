// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Microsoft.EntityFrameworkCore;

    public class BaseInternalUnitResultEventHandler : BaseInternalEventHandler
    {
        public BaseInternalUnitResultEventHandler(IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        private const int PercentOccupyVideo = 18;
        private const int PercentOccupySkillTest = 10;
        private const int PercentOccupyUnitTest = 30;
        private const int PercentOccupyHomeWork = 22;
        private const int PercentOccupyClassForum = 20;

        public async Task UpdateUnitResultAsync(IList<Guid>? lessonResultIds, Unit? unit, Guid courseId, Guid studentId, bool isDone, CancellationToken cancellationToken)

        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            ArgumentNullException.ThrowIfNull(unit);
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (unit != null && unitResult != null)
            {
                var (skillScores, percent) = await GetUnitSkillScores(lessonResultIds);
                if (isDone)
                {
                    unitResult.Status = EnumResultStatus.Done;
                    await _finishOneUnitPublisher.Publish(unitResult, cancellationToken);
                }
                unitResult.CorrectCount = (int)skillScores.Sum(x => x.CorrectCount);
                unitResult.CorrectTotal = (int)skillScores.Sum(x => x.TotalCount);
                unitResult.Percent = percent;
                unitResult.SkillScores = skillScores;
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<(List<SkillScores>, double)> GetUnitSkillScores(IList<Guid>? lessonResultIds)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            var (videoSkillScores, percentVideo) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, PercentOccupyVideo);
            var (unitTestSkillScores, percentUnitTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, PercentOccupyUnitTest);
            if (!unitTestSkillScores.Any())
            {
                percentUnitTest = PercentOccupyUnitTest;
            }
            var (skillTestSkillScores, percentSkillTest) = await GetVideoSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest, PercentOccupySkillTest);
            if (!skillTestSkillScores.Any())
            {
                percentSkillTest = PercentOccupySkillTest;
            }
            var (homeWorkSkillScores, percentHomeWork) = await GetHomeWordsSkillScores(lessonResultIds, PercentOccupyHomeWork);
            var (classForumSkillScores, percentClassForum) = await GetClassForumSkillScores(lessonResultIds, PercentOccupyClassForum);
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).Concat(skillTestSkillScores).Concat(unitTestSkillScores).ToList();
            List<SkillScores> groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
            var percents = new List<double> { percentClassForum, percentHomeWork, percentSkillTest, percentUnitTest, percentVideo };
            return (groupedSkillScores, (int)percents.Sum());
        }
    }
}
