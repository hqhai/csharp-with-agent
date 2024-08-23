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

    public class UnitResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<UnitResult>>
    {
        public UnitResultInputThenUpdateCourseResultHandler(ISystemService systemService, AppSetting appSetting, ICourseUnitMockTestRepository courseUnitMockTestRepository, IMediator mediator, IUserService userService, SaveUserCourseSettingPublisher saveUserCourseSettingPublisher, IVideoResultRepository videoResultRepository, IClassForumResultRepository classForumResultRepository, IUnitResultRepository unitResultRepository, ICourseResultRepository courseResultRepository, ICourseRepository courseRepository, IUnitRepository unitRepository, IFinalTestResultRepository finalTestResultRepository, IMockTestResultRepository mockTestResultRepository, IHomeWorkResultRepository homeWorkResultRepository, QuestBoardPublisher questBoardPublisher, IOrderService orderService) : base(systemService, appSetting, courseUnitMockTestRepository, mediator, userService, saveUserCourseSettingPublisher, videoResultRepository, classForumResultRepository, unitResultRepository, courseResultRepository, courseRepository, unitRepository, finalTestResultRepository, mockTestResultRepository, homeWorkResultRepository, questBoardPublisher, orderService)
        {
        }

        public async Task Handle(EntityChangedEvent<UnitResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unitResult = notification.Data;
            await UpdateCourse(unitResult.Course, unitResult.StudentId, cancellationToken);
            Thread.Sleep(2000);
            if (unitResult.Status == EnumResultStatus.Done)
            {
                await UpdateProcessUnit(unitResult, cancellationToken);
            }
        }
    }
}
