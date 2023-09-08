// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
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

        public async Task UpdateUnit(IList<Guid>? lessonResultIds, Domain.Entities.Unit? unit, Guid courseId, Guid studentId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            ArgumentNullException.ThrowIfNull(unit);
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == unit.Id && x.StudentId == studentId && x.CourseId == courseId, cancellationToken);
            if (unit != null && unitResult != null)
            {
                var (groupedSkillScores, percent) = await UpdateUnitResult(lessonResultIds);
                unitResult.CorrectCount = (int)groupedSkillScores.Sum(x => x.CorrectCount);
                unitResult.CorrectTotal = (int)groupedSkillScores.Sum(x => x.TotalCount);
                unitResult.Status = EnumResultStatus.Done;
                unitResult.Percent = percent;
                unitResult.SkillScores = groupedSkillScores;
                await _finishOneUnitPublisher.Publish(unitResult, cancellationToken);
                _unitResultRepository.Update(unitResult);
                await _unitResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<(List<SkillScores>, double)> UpdateUnitResult(IList<Guid>? lessonResultIds)
        {
            ArgumentNullException.ThrowIfNull(lessonResultIds);
            var (videoSkillScores, percentVideo) = await VideoSkillScores(lessonResultIds, EnumTimeCodeType.Standalone, 18);
            var (unitTestSkillScores, percentUnitTest) = await VideoSkillScores(lessonResultIds, EnumTimeCodeType.UnitTest, 30);
            var (skillTestSkillScores, percentSkillTest) = await VideoSkillScores(lessonResultIds, EnumTimeCodeType.SkillTest, 10);
            var (homeWorkSkillScores, percentHomeWork) = await HomeWordsSkillScores(lessonResultIds, 22);
            var (classForumSkillScores, percentClassForum) = await ClassForumSkillScores(lessonResultIds, 20);
            List<SkillScores> mergedSkillScores = videoSkillScores.Concat(homeWorkSkillScores).Concat(classForumSkillScores).Concat(skillTestSkillScores).Concat(unitTestSkillScores).ToList();
            List<SkillScores> groupedSkillScores = mergedSkillScores.GroupBy(x => x.Skill).Select(group => GetSumSkillScore(group)).ToList();
            var percent = percentClassForum + percentHomeWork + percentSkillTest + percentUnitTest + percentVideo;
            return (groupedSkillScores, percent);
        }
    }
}
