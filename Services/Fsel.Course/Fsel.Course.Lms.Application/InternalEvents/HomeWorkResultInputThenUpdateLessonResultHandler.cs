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

    public class HomeWorkResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<HomeWorkResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILogger<HomeWorkResultInputThenUpdateLessonResultHandler> _logger;

        public HomeWorkResultInputThenUpdateLessonResultHandler(ISystemService systemService,
                                                                AppSetting appSetting,
                                                                ICourseUnitMockTestRepository courseUnitMockTestRepository,
                                                                IMediator mediator,
                                                                IUserService userService,
                                                                ILogger<HomeWorkResultInputThenUpdateLessonResultHandler> logger,
                                                                ILessonResultRepository lessonResultRepository,
                                                                SaveUserCourseSettingPublisher saveUserCourseSettingPublisher,
                                                                IVideoResultRepository videoResultRepository,
                                                                IClassForumResultRepository classForumResultRepository,
                                                                IUnitResultRepository unitResultRepository,
                                                                ICourseResultRepository courseResultRepository,
                                                                ICourseRepository courseRepository,
                                                                IUnitRepository unitRepository,
                                                                IFinalTestResultRepository finalTestResultRepository,
                                                                IMockTestResultRepository mockTestResultRepository,
                                                                IHomeWorkResultRepository homeWorkResultRepository,
                                                                QuestBoardPublisher questBoardPublisher,
                                                                IOrderService orderService,
                                                                ILessonNoteRepository lessonNoteRepository,
                                                                NotificationMessagePublisher notificationMessagePublisher,
                                                                IStudentGoalAggregateRepository studentGoalAggregateRepository,
                                                                IStudentGoalSummaryRepository studentGoalSummaryRepository) : base(systemService,
                                                                                                                                  appSetting,
                                                                                                                                  courseUnitMockTestRepository,
                                                                                                                                  mediator,
                                                                                                                                  userService,
                                                                                                                                  logger,
                                                                                                                                  lessonResultRepository,
                                                                                                                                  saveUserCourseSettingPublisher,
                                                                                                                                  videoResultRepository,
                                                                                                                                  classForumResultRepository,
                                                                                                                                  unitResultRepository,
                                                                                                                                  courseResultRepository,
                                                                                                                                  courseRepository,
                                                                                                                                  unitRepository,
                                                                                                                                  finalTestResultRepository,
                                                                                                                                  mockTestResultRepository,
                                                                                                                                  homeWorkResultRepository,
                                                                                                                                  questBoardPublisher,
                                                                                                                                  orderService,
                                                                                                                                  lessonNoteRepository,
                                                                                                                                  notificationMessagePublisher,
                                                                                                                                  studentGoalAggregateRepository,
                                                                                                                                  studentGoalSummaryRepository)
        {
            _lessonResultRepository = lessonResultRepository;
            _logger = logger;
        }

        public async Task Handle(EntityChangedEvent<HomeWorkResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var homeWorkResult = notification.Data;

            try
            {
                var isHomeWorkOtherDone = await _homeWorkResultRepository.Queryable.AnyAsync(x => x.LessonResultId == homeWorkResult.LessonResultId && x.Status != EnumResultStatus.Done, cancellationToken);
                if (isHomeWorkOtherDone)
                {
                    return;
                }
                var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.HomeWorkResults.Where(x => x.LessonResultId == homeWorkResult.LessonResultId))
                                                                         .Include(x => x.ClassForumResults.Where(x => x.LessonResultId == homeWorkResult.LessonResultId))
                                                                         .FirstOrDefaultAsync(x => x.Id == homeWorkResult.LessonResultId, cancellationToken);
                if (lessonResult == null)
                {
                    return;
                }
                await UpdateLessonResultAsync(lessonResult, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Trigger HomeWorkResult : {ex.Message} ");
            }
        }
    }
}
