// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using MediatR;

    public class FinalResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<FinalTestResult>>
    {
        public FinalResultInputThenUpdateCourseResultHandler(
             ILessonResultRepository lessonResultRepository
            , IUnitRepository unitRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , ICourseRepository courseRepository
            , FinishOneUnitPublisher finishOneUnitPublisher
            , ICourseResultRepository courseResultRepository
            , IUnitResultRepository unitResultRepository
            , FinishOneLevelPassPublisher finishOneLevelPassPublisher
            , IMockTestResultRepository mockTestResultRepository
            , IFinalTestResultRepository finalTestResultRepository
            ) : base(videoResultRepository,
                classForumResultRepository,
                unitResultRepository,
                lessonResultRepository,
                courseResultRepository,
                courseRepository,
                unitRepository,
                finishOneUnitPublisher,
                finishOneLevelPassPublisher,
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
                await UpdateCourseResult(finalTestResult.CourseId, finalTestResult.StudentId, cancellationToken);
            }
        }
    }
}
