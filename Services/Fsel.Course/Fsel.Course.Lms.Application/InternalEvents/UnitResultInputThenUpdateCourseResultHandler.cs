// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Queues.Publishers;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UnitResultInputThenUpdateCourseResultHandler : BaseInternalEventHandler,
        INotificationHandler<EntityChangedEvent<UnitResult>>
    {
        private readonly ICourseRepository _courseRepository;

        public UnitResultInputThenUpdateCourseResultHandler(IUnitResultRepository unitResultRepository
            , ILessonResultRepository lessonResultRepository
            , IVideoResultRepository videoResultRepository
            , IClassForumResultRepository classForumResultRepository
            , IHomeWorkResultRepository homeWorkResultRepository
            , ICourseRepository courseRepository
            , FinishOneUnitPublisher finishOneUnitPublisher
            , ICourseResultRepository courseResultRepository
            , IMockTestResultRepository mockTestResultRepository
            , FinishOneLevelPassPublisher finishOneLevelPassPublisher
            , IFinalTestResultRepository finalTestResultRepository
            ) : base(videoResultRepository,
                classForumResultRepository,
                unitResultRepository,
                lessonResultRepository,
                courseResultRepository,
                courseRepository,
                finishOneUnitPublisher,
                finishOneLevelPassPublisher,
                finalTestResultRepository,
                mockTestResultRepository,
                homeWorkResultRepository)
        {
            _courseRepository = courseRepository;
        }

        public async Task Handle(EntityChangedEvent<UnitResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unitResult = notification.Data;
            if (unitResult.Status == EnumResultStatus.Done)
            {
                await UpdateProcessUnit(unitResult, cancellationToken);
            }
        }

        public async Task UpdateCourse(Guid courseId, CancellationToken cancellationToken)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course != null)
            {
                var courseType = course.CourseLevel.GetEnumCourseType();
                if (courseType == EnumCourseType.Academic)
                {
                }
                else
                {
                }
            }
        }
    }
}
