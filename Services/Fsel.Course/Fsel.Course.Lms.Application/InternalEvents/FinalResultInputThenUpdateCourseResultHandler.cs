// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;

    public class FinalResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<FinalTestResult>>
    {
        public FinalResultInputThenUpdateCourseResultHandler(
             ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , ICourseRepository courseRepository
            , IUnitResultRepository unitResultRepository
            , IMockTestResultRepository mockTestResultRepository
            , IFinalTestResultRepository finalTestResultRepository
            ) : base(videoResultRepository,
                classForumResultRepository,
                unitResultRepository,
                lessonResultRepository,
                courseRepository,
                finalTestResultRepository,
                mockTestResultRepository,
                homeWorkResultRepository)
        {
        }

        public async Task Handle(EntityChangedEvent<FinalTestResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var finalTestResult = notification.Data;
            if (finalTestResult != null && finalTestResult.Status == EnumResultStatus.Done)
            {
                await UpdateProcessFinalTest(finalTestResult, cancellationToken);
            }
        }
    }
}
