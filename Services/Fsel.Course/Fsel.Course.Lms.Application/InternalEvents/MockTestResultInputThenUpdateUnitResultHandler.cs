// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class MockTestResultInputThenUpdateUnitResultHandler : BaseInternalUnitResultEventHandler,
        INotificationHandler<EntityChangedEvent<MockTestResult>>
    {
        public MockTestResultInputThenUpdateUnitResultHandler(IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ILessonResultRepository lessonResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IMockTestRepository mockTestRepository, IHomeWorkQuestionRepository homeWorkQuestionRepository, IHomeWorkAnswerRepository homeWorkAnswerRepository, IQuestionRepository questionRepository, IHomeWorkRepository homeWorkRepository, FinishOneUnitPublisher finishOneUnitPublisher, FinishOneLevelPassPublisher finishOneLevelPassPublisher, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(videoResultRepository, classForumResultRepository, unitResultRepository, lessonResultRepository, courseResultRepository, courseRepository, unitRepository, mockTestRepository, homeWorkQuestionRepository, homeWorkAnswerRepository, questionRepository, homeWorkRepository, finishOneUnitPublisher, finishOneLevelPassPublisher, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task Handle(EntityChangedEvent<MockTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var mockTestResult = notification.Data;
            var mockTest = await _mockTestRepository.GetByIdAsync(mockTestResult.MockTestId);

            if (mockTest != null && mockTest.MockTestType == EnumMockTestType.SkillMockTest && mockTestResult.Status == EnumResultStatus.Done)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                   .Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId))
                                                   .FirstOrDefaultAsync(x => x.Id == mockTestResult.UnitId, cancellationToken);
                if (unit != null)
                {
                    var lessonResulIds = unit.LessonResults.Select(x => x.Id).ToList();
                    var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == mockTestResult.UnitId && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId, cancellationToken);
                    if (unitResult != null && unit.LessonResults.Count == unit.UnitLessons.Count)
                    {
                        await UpdateUnit(lessonResulIds, unit, mockTestResult.CourseId, mockTestResult.StudentId, cancellationToken);
                    }
                }
            }
            else if (mockTest != null && mockTest.MockTestType == EnumMockTestType.SkillMockTest && mockTestResult.Status == EnumResultStatus.Done)
            {
                await UpdateProcessMockTest(mockTestResult, cancellationToken);
            }
        }
    }
}
