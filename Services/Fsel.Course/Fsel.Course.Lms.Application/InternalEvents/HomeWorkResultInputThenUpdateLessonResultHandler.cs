// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.ValueSettings;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class HomeWorkResultInputThenUpdateLessonResultHandler : BaseInternalLessonResultEventHandler,
        INotificationHandler<EntityChangedEvent<HomeWorkResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

        public HomeWorkResultInputThenUpdateLessonResultHandler(FinishOneLessonPublisher finishOneLessonPublisher, ILessonResultRepository lessonResultRepository, ISystemService systemService, AppSetting appSetting, FinishOneLevelPassPublisher finishOneLevelPassPublisher, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(finishOneLessonPublisher, lessonResultRepository, systemService, appSetting, finishOneLevelPassPublisher, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
            _lessonResultRepository = lessonResultRepository;
        }

        public async Task Handle(EntityChangedEvent<HomeWorkResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var homeWorkResult = notification.Data;
            var lessonResult = await _lessonResultRepository.Queryable.Include(x => x.HomeWorkResults.Where(x => x.StudentId == homeWorkResult.StudentId && x.LessonResultId == homeWorkResult.LessonResultId))
                                                                        .Include(x => x.ClassForumResults.Where(x => x.StudentId == homeWorkResult.StudentId && x.LessonResultId == homeWorkResult.LessonResultId))
                                                                        .FirstOrDefaultAsync(x => x.Id == homeWorkResult.LessonResultId, cancellationToken);
            if (lessonResult != null)
            {
                await UpdateLessonResultAsync(lessonResult, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
