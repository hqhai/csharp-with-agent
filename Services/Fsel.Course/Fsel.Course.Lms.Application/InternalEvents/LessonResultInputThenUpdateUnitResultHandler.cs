// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Unit = Course.Domain.Entities.Unit;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseInternalUnitResultEventHandler,
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        public LessonResultInputThenUpdateUnitResultHandler(IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = notification.Data;
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == lessonResult.StudentId && x.CourseId == lessonResult.CourseId))
                                                    .Include(x => x.UnitLessons)
                                                    .ThenInclude(x => x.Lesson)
                                                    .Include(x => x.UnitSkillMockTests)
                                                    .FirstOrDefaultAsync(x => x.Id == lessonResult.UnitId, cancellationToken);

            if (unit != null && lessonResult.Status == EnumResultStatus.Done)
            {
                var lessonResultIds = unit.LessonResults.Select(x => x.Id).ToList();
                if (unit.LessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count == 0)
                {
                    await UpdateUnit(lessonResultIds, unit, lessonResult.CourseId, lessonResult.StudentId, cancellationToken);
                }
                else if (unit.LessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count > 0)
                {
                    await UpdateUnit(lessonResultIds, unit, lessonResult.CourseId, lessonResult.StudentId, cancellationToken).ConfigureAwait(false);
                    await UpdateTheNextLesson(unit, lessonResult, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var isCheck = unit.LessonResults.Any(x => x.Status == EnumResultStatus.New);
                    if (!isCheck)
                    {
                        await UpdateUnit(lessonResultIds, unit, lessonResult.CourseId, lessonResult.StudentId, cancellationToken).ConfigureAwait(false);
                        await UpdateTheNextLesson(unit, lessonResult, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task UpdateTheNextLesson(Unit? unit, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unit);
            var mockTestId = unit.UnitSkillMockTests.FirstOrDefault()?.MockTestId;
            var displayOrder = unit.UnitLessons.FirstOrDefault(x => x.LessonId == lessonResult.LessonId)!.DisplayOrder;
            var lesson = unit.UnitLessons.FirstOrDefault(x => x.DisplayOrder == displayOrder + 1)?.Lesson;
            if (lesson != null)
            {
                var lessonResultNext = await _lessonResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.LessonId == lesson.Id, cancellationToken);
                if (lessonResultNext != null)
                {
                    lessonResultNext.Status = EnumResultStatus.New;
                    _lessonResultRepository.Update(lessonResultNext);
                    await _lessonResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            else if (unit.CourseLevel.GetEnumCourseType() == EnumCourseType.Ielts && mockTestId.HasValue)
            {
                var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseId == lessonResult.CourseId && x.UnitId == lessonResult.UnitId && x.StudentId == lessonResult.StudentId && x.MockTestId == mockTestId.Value, cancellationToken);
                if (mockTestResult != null)
                {
                    mockTestResult.Status = EnumResultStatus.New;
                    _mockTestResultRepository.Update(mockTestResult);
                    await _mockTestResultRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
