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
    using Fsel.Course.Lms.Application.Services.SystemService;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;

    public class FinalResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<FinalTestResult>>
    {
        public FinalResultInputThenUpdateCourseResultHandler(ISystemService systemService, AppSetting appSetting, FinishOneLevelPassPublisher finishOneLevelPassPublisher, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository) : base(systemService, appSetting, finishOneLevelPassPublisher, courseUnitMockTestRepository, mediator, userService, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository)
        {
        }

        public async Task Handle(EntityChangedEvent<FinalTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var finalTestResult = notification.Data;
            if (finalTestResult != null && finalTestResult.Status == EnumResultStatus.Done)
            {
                await UpdateCourseResult(finalTestResult.CourseId, finalTestResult.StudentId, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
