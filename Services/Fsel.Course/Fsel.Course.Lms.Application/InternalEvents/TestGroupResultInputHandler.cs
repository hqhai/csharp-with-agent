// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.InternalEvents.BaseCourseModule;
    using Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule;
    using MediatR;

    public class TestGroupResultInputHandler : INotificationHandler<EntityChangedEvent<TestGroupResult>>
    {
        private readonly ICourseResultUpdater _courseResultUpdater;
        private readonly IUnitResultUpdater _unitResultUpdater;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public TestGroupResultInputHandler(ICourseResultUpdater courseResultUpdater,
            IUnitResultUpdater unitResultUpdater,
            IUnitResultRepository unitResultRepository,
            ICourseResultRepository courseResultRepository)
        {
            _courseResultUpdater = courseResultUpdater;
            _unitResultUpdater = unitResultUpdater;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
        }

        public async Task Handle(EntityChangedEvent<TestGroupResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var testGroupResult = notification.Data;

            try
            {
                if (testGroupResult.UnitResultId.HasValue)
                {
                    var unitResult = await _unitResultRepository.GetByIdAsync(testGroupResult.UnitResultId.Value);
                    if (unitResult == null || !testGroupResult.UnitModuleId.HasValue || testGroupResult.Status != EnumResultStatus.Done)
                    {
                        return;
                    }
                    await _unitResultUpdater.UpdateUnitResultAsync(unitResult, testGroupResult.UnitModuleId.Value, cancellationToken);
                }
                else if (testGroupResult.CourseResultId.HasValue)
                {
                    var courseResult = await _courseResultRepository.GetByIdAsync(testGroupResult.CourseResultId.Value);
                    if (courseResult == null || !testGroupResult.CourseModuleId.HasValue || testGroupResult.Status != EnumResultStatus.Done)
                    {
                        return;
                    }
                    await _courseResultUpdater.UpdateCourseResultAsync(courseResult, testGroupResult.CourseModuleId.Value, cancellationToken);
                }
            }
            catch
            {
            }
        }
    }
}
