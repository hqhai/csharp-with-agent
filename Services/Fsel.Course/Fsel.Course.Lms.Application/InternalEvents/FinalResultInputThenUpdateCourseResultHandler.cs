// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
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

    public class FinalResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<FinalTestResult>>
    {
        private readonly ILogger<FinalResultInputThenUpdateCourseResultHandler> _logger;

        public FinalResultInputThenUpdateCourseResultHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ILogger<FinalResultInputThenUpdateCourseResultHandler> logger, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, IOrderService orderService, ILessonNoteRepository lessonNoteRepository, ILessonResultRepository lessonResultRepository, NotificationMessagePublisher notificationMessagePublisher, SendMailFinishCoursePublisher sendMailFinishCoursePublisher) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, logger, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, orderService, lessonNoteRepository, lessonResultRepository, notificationMessagePublisher, sendMailFinishCoursePublisher)
        {
            _logger = logger;
        }

        public async Task Handle(EntityChangedEvent<FinalTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var finalTestResult = notification.Data;
            try
            {
                var course = await _courseRepository.Queryable.Include(x => x.CourseUnitMockTests).Include(x => x.CourseResults.Where(x => x.StudentId == finalTestResult.StudentId))
                               .FirstOrDefaultAsync(x => x.Id == finalTestResult.CourseId, cancellationToken);
                if (finalTestResult == null || finalTestResult.Status != EnumResultStatus.Done)
                {
                    return;
                }
                await UpdateCourseResult(course, finalTestResult.StudentId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger FinalTestResult : {ex.Message} ");
            }
        }
    }
}
