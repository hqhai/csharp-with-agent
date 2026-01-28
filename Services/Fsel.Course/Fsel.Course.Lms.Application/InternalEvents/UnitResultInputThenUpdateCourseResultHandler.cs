// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
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

    public class UnitResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<UnitResult>>
    {
        private readonly ILogger<UnitResultInputThenUpdateCourseResultHandler> _logger;

        public UnitResultInputThenUpdateCourseResultHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ILogger<UnitResultInputThenUpdateCourseResultHandler> logger, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, IOrderService orderService, ILessonNoteRepository lessonNoteRepository, ILessonResultRepository lessonResultRepository, NotificationMessagePublisher notificationMessagePublisher, SendMailFinishCoursePublisher sendMailFinishCoursePublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, logger, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, orderService, lessonNoteRepository, lessonResultRepository, notificationMessagePublisher, sendMailFinishCoursePublisher)
        {
            _logger = logger;
        }

        public async Task Handle(EntityChangedEvent<UnitResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unitResult = notification.Data;
            try
            {
                var courseResult = await _courseResultRepository.Queryable.Where(x => x.CourseId == unitResult.CourseId && x.StudentId == unitResult.StudentId).FirstOrDefaultAsync(cancellationToken);
                if (courseResult != null)
                {
                    if (courseResult.Status == EnumResultStatus.Done)
                    {
                        await UpdateCourseResult(unitResult.Course, unitResult.StudentId, cancellationToken);
                    }
                    else
                    {
                        await UpdateCourse(unitResult.Course, unitResult.StudentId, cancellationToken);
                    }
                }
                if (unitResult.Status != EnumResultStatus.Done)
                {
                    return;
                }
                await UpdateProcessUnit(unitResult, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger UnitResult : {ex.Message} ");
            }
        }
    }
}
