// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseCourseModule
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices;
    using MediatR;

    public class UnitResultInputThenUpdateCourseResultHandler : BaseCourseResultEventHandler, INotificationHandler<EntityChangedEvent<UnitResult>>
    {
        private readonly ICourseResultRepository _courseResultRepository;

        public UnitResultInputThenUpdateCourseResultHandler(ICourseModuleCachingService courseModuleCachingService,
            ICourseModuleRepository courseModuleRepository,
            ICourseResultRepository courseResultRepository,
            ICourseItemInitializerFactory courseItemInitializerFactory) : base(courseModuleCachingService, courseModuleRepository, courseResultRepository, courseItemInitializerFactory)
        {
            _courseResultRepository = courseResultRepository;
        }

        public async Task Handle(EntityChangedEvent<UnitResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var unitResult = notification.Data;

            if (!unitResult.CourseResultId.HasValue)
            {
                return;
            }

            try
            {
                var courseResult = await _courseResultRepository.GetByIdAsync(unitResult.CourseResultId.Value);
                if (courseResult == null || !unitResult.CourseModuleId.HasValue || unitResult.Status != EnumResultStatus.Done)
                {
                    return;
                }
                await UpdateCourseResultAsync(courseResult, unitResult.CourseModuleId.Value, cancellationToken);
            }
            catch
            {
            }
        }
    }
}
