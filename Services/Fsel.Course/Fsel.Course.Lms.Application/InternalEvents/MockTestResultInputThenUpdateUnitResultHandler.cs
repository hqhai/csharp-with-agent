// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class MockTestResultInputThenUpdateUnitResultHandler : BaseInternalUnitResultEventHandler,
        INotificationHandler<EntityChangedEvent<MockTestResult>>
    {
        private readonly IMockTestRepository _mockTestRepository;

        public MockTestResultInputThenUpdateUnitResultHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, ITrainingService trainingService, IMockTestRepository mockTestRepository, QuestBoardPublisher questBoardPublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, trainingService, questBoardPublisher)
        {
            _mockTestRepository = mockTestRepository;
        }

        public async Task Handle(EntityChangedEvent<MockTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var mockTestResult = notification.Data;
            var mockTest = await _mockTestRepository.Queryable.Where(x => x.Id == mockTestResult.MockTestId).FirstOrDefaultAsync(cancellationToken);

            if (mockTest != null && mockTest.MockTestType == EnumMockTestType.SkillMockTest && mockTestResult.Status == EnumResultStatus.Done)
            {
                var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                   .Include(x => x.UnitResults.Where(x => x.UnitId == mockTestResult.UnitId && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId))
                                                   .Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId))
                                                   .FirstOrDefaultAsync(x => x.Id == mockTestResult.UnitId, cancellationToken);
                if (unit != null)
                {
                    var lessonResulIds = unit.LessonResults.Select(x => x.Id).ToList();
                    var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == mockTestResult.UnitId && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId, cancellationToken);
                    if (unitResult != null && unit.LessonResults.Count == unit.UnitLessons.Count)
                    {
                        await UpdateUnitResultAsync(lessonResulIds, unit, mockTestResult.CourseId, mockTestResult.StudentId, true, cancellationToken);
                    }
                }
            }
            else if (mockTest != null && mockTest.MockTestType == EnumMockTestType.FullMockTest && mockTestResult.Status == EnumResultStatus.Done)
            {
                await UpdateProcessMockTest(mockTestResult, cancellationToken);
            }
        }
    }
}
