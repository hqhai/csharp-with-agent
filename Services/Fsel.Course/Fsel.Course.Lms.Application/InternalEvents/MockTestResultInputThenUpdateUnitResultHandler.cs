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
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class MockTestResultInputThenUpdateUnitResultHandler : BaseInternalUnitResultEventHandler,
        INotificationHandler<EntityChangedEvent<MockTestResult>>
    {
        private readonly IMockTestRepository _mockTestRepository;

        public MockTestResultInputThenUpdateUnitResultHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, ILessonResultRepository lessonResultRepository, IMockTestRepository mockTestRepository, IOrderService orderService, ILessonNoteRepository noteRepository, ILogger<BaseInternalUnitResultEventHandler> logger, NotificationMessagePublisher notificationMessagePublisher, SaveUserSurveyAssignmentPublisher saveUserSurveyAssignmentPublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, lessonResultRepository, orderService, noteRepository, logger, notificationMessagePublisher, saveUserSurveyAssignmentPublisher)
        {
            _mockTestRepository = mockTestRepository;
        }

        public async Task Handle(EntityChangedEvent<MockTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var mockTestResult = notification.Data;
            try
            {
                var mockTest = await _mockTestRepository.GetByIdAsync(mockTestResult.MockTestId);
                if (mockTest == null || mockTestResult.Status != EnumResultStatus.Done)
                {
                    return;
                }
                if (mockTest.MockTestType == EnumMockTestType.SkillMockTest)
                {
                    var unit = await _unitRepository.Queryable.Include(x => x.UnitLessons)
                                                       .Include(x => x.UnitResults.Where(x => x.UnitId == mockTestResult.UnitId && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId))
                                                       .Include(x => x.LessonResults.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId))
                                                       .FirstOrDefaultAsync(x => x.Id == mockTestResult.UnitId, cancellationToken);
                    if (unit == null)
                    {
                        return;
                    }
                    var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == mockTestResult.UnitId && x.StudentId == mockTestResult.StudentId && x.CourseId == mockTestResult.CourseId, cancellationToken);
                    if (unitResult == null || unit.LessonResults.Count != unit.UnitLessons.Count)
                    {
                        return;
                    }
                    await UpdateUnitResultAsync(unit.LessonResults.ToList(), unit, mockTestResult.CourseId, mockTestResult.StudentId, true, cancellationToken);
                }
                else
                {
                    await UpdateProcessMockTest(mockTestResult, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger MockTestResult : {ex.Message} ");
            }
        }
    }
}
