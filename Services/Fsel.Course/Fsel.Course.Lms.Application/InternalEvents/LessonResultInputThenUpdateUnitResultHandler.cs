// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SenderService;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Unit = Course.Domain.Entities.Unit;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseInternalUnitResultEventHandler,
        INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        public LessonResultInputThenUpdateUnitResultHandler(ISystemService systemService, AppSetting appSetting, ITrainingService trainingService, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ISenderService senderService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(systemService, appSetting, trainingService, courseUnitMockTestRepository, mediator, userService, senderService, videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = notification.Data;
            var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                    .ThenInclude(x => x.Lesson)
                                                    .Include(x => x.UnitSkillMockTests)
                                                    .FirstOrDefaultAsync(x => x.Id == lessonResult.UnitId, cancellationToken);

            var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && (!(unit != null) || x.UnitId == unit.Id) && x.StudentId == lessonResult.StudentId && x.CourseId == lessonResult.CourseId).ToListAsync(cancellationToken);
            if (unit != null && lessonResult.Status == EnumResultStatus.Done)
            {
                var lessonResultIds = lessonResults.Select(x => x.Id).ToList();
                if (lessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count == 0)
                {
                    await UpdateUnit(lessonResultIds, unit, lessonResult.CourseId, lessonResult.StudentId, true, cancellationToken);
                }
                else if (lessonResults.Count == unit.UnitLessons.Count && unit.UnitSkillMockTests.Count > 0)
                {
                    await UpdateUnit(lessonResultIds, unit, lessonResult.CourseId, lessonResult.StudentId, true, cancellationToken).ConfigureAwait(false);
                    await UpdateTheNextLesson(unit, lessonResult, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    var isCheck = lessonResults.Any(x => x.Status == EnumResultStatus.New);
                    if (!isCheck)
                    {
                        await UpdateUnit(lessonResultIds, unit, lessonResult.CourseId, lessonResult.StudentId, false, cancellationToken).ConfigureAwait(false);
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
