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

    public class ClassForumResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<ClassForumResult>>, INotificationHandler<EntityCreatedEvent<ClassForumResult>>
    {
        private readonly ILogger<ClassForumResultInputThenUpdateLessonResultHandler> _logger2;
        private readonly ILessonResultRepository _lessonResultRepository;

        public ClassForumResultInputThenUpdateLessonResultHandler(ISystemService systemService, ILogger<ClassForumResultInputThenUpdateLessonResultHandler> logger2, AppSetting appSetting, ILogger<BaseInternalLessonResultEventHandler> logger1, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, ILogger<BaseInternalEventHandler> logger, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, IOrderService orderService, ILessonNoteRepository lessonNoteRepository, ILessonResultRepository lessonResultRepository) : base(systemService, appSetting, logger1, courseUnitMockTestRepository, mediator, userService, logger, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, orderService, lessonNoteRepository, lessonResultRepository)
        {
            _logger2 = logger2;
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<ClassForumResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await ExecuteEvent(notification.Data, cancellationToken);
        }

        public async Task Handle(EntityCreatedEvent<ClassForumResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            await ExecuteEvent(notification.Data, cancellationToken);
        }

        private async Task ExecuteEvent(ClassForumResult classForumResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(classForumResult);
            try
            {
                var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.HomeWorkResults.Where(x => x.StudentId == classForumResult.StudentId && x.LessonResultId == classForumResult.LessonResultId))
                                                                             .Include(x => x.ClassForumResults.Where(x => x.StudentId == classForumResult.StudentId && x.LessonResultId == classForumResult.LessonResultId))
                                                                             .FirstOrDefaultAsync(x => x.Id == classForumResult.LessonResultId, cancellationToken);
                if (lessonResult == null)
                {
                    return;
                }

                await UpdateHomeWorksAsync(classForumResult, cancellationToken);
                await UpdateLessonResultAsync(lessonResult, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger2.LogWarning($"Log Trigger ClassForumResult : {ex.Message} ");
            }
        }

        private async Task UpdateHomeWorksAsync(ClassForumResult classForumResult, CancellationToken cancellationToken)
        {
            var homeWorkResults = await _homeWorkResultRepository.Queryable.Where(x => x.LessonResultId == classForumResult.LessonResultId && x.Status == EnumResultStatus.Unfinished).ToListAsync(cancellationToken);
            if (homeWorkResults == null || !homeWorkResults.Any())
            {
                return;
            }
            homeWorkResults = homeWorkResults.Select(x => { x.Status = EnumResultStatus.New; return x; }).ToList();
            _homeWorkResultRepository.UpdateList(homeWorkResults);
            await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
