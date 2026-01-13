// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.InternalEvents.BaseCourseModule;
    using Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class TestGroupResultInputHandler : INotificationHandler<EntityChangedEvent<TestGroupResult>>
    {
        private readonly ICourseResultUpdater _courseResultUpdater;
        private readonly IUnitResultUpdater _unitResultUpdater;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly ICourseModuleRepository _courseModuleRepository;

        public TestGroupResultInputHandler(ICourseResultUpdater courseResultUpdater,
            IUnitResultUpdater unitResultUpdater,
            IUnitResultRepository unitResultRepository,
            ICourseResultRepository courseResultRepository,
            ITestResultRepository testResultRepository,
            IUnitModuleRepository unitModuleRepository,
            ICourseModuleRepository courseModuleRepository)
        {
            _courseResultUpdater = courseResultUpdater;
            _unitResultUpdater = unitResultUpdater;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _testResultRepository = testResultRepository;
            _unitModuleRepository = unitModuleRepository;
            _courseModuleRepository = courseModuleRepository;
        }

        public async Task Handle(EntityChangedEvent<TestGroupResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var testGroupResult = notification.Data;

            try
            {
                var testResult = await _testResultRepository.Queryable.FirstOrDefaultAsync(x => x.TestGroupResultId == testGroupResult.Id, cancellationToken);

                if (testGroupResult.UnitResultId.HasValue)
                {
                    var unitResult = await _unitResultRepository.GetByIdAsync(testGroupResult.UnitResultId.Value);
                    if (unitResult == null || !testGroupResult.UnitModuleId.HasValue || testGroupResult.Status != EnumResultStatus.Done)
                    {
                        return;
                    }
                    var percentModule = await _unitModuleRepository.ReadQueryable
                                        .Where(x => x.Id == testGroupResult.UnitModuleId)
                                        .Select(x => x.Percent)
                                        .FirstOrDefaultAsync(cancellationToken);
                    if (unitResult.Status != EnumResultStatus.Done)
                    {
                        await UpdateTestResultAsync(testResult, percentModule, cancellationToken);
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

                    var percentModule = await _courseModuleRepository.ReadQueryable
                                       .Where(x => x.Id == testGroupResult.CourseModuleId)
                                       .Select(x => x.Percent)
                                       .FirstOrDefaultAsync(cancellationToken);
                    if (courseResult.Status != EnumResultStatus.Done)
                    {
                        await UpdateTestResultAsync(testResult, percentModule, cancellationToken);
                    }

                    await _courseResultUpdater.UpdateCourseResultAsync(courseResult, testGroupResult.CourseModuleId.Value, cancellationToken);
                }
            }
            catch
            {
            }
        }

        private async Task UpdateTestResultAsync(TestResult? testResult, double percentModule, CancellationToken cancellationToken)
        {
            if (testResult == null)
            {
                return;
            }

            testResult.PercentModule = NumberHelper.ConvertDoublePercent(testResult.Percent * percentModule, 2);
            await _testResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
