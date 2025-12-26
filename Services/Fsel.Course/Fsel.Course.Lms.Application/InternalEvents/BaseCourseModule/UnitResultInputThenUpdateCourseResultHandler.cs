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
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class UnitResultInputThenUpdateCourseResultHandler : BaseCourseResultEventHandler, INotificationHandler<EntityChangedEvent<UnitResult>>
    {
        private readonly ICourseModuleRepository _courseModuleRepository;
        private readonly ICourseResultRepository _courseResultRepository;
        private readonly IUnitResultRepository _unitResultRepository;

        public UnitResultInputThenUpdateCourseResultHandler(ICourseModuleCachingService courseModuleCachingService,
            ICourseModuleRepository courseModuleRepository,
            ICourseResultRepository courseResultRepository,
            ICourseItemInitializerFactory courseItemInitializerFactory,
            ITestGroupResultRepository testGroupResultRepository,
            IUnitResultRepository unitResultRepository) : base(courseModuleCachingService, courseModuleRepository, courseResultRepository, courseItemInitializerFactory, testGroupResultRepository, unitResultRepository)
        {
            _courseModuleRepository = courseModuleRepository;
            _courseResultRepository = courseResultRepository;
            _unitResultRepository = unitResultRepository;
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

                var percentModule = await _courseModuleRepository.ReadQueryable
                                        .Where(x => x.Id == unitResult.CourseModuleId)
                                        .Select(x => x.Percent)
                                        .FirstOrDefaultAsync(cancellationToken);
                if (courseResult.Status != EnumResultStatus.Done)
                {
                    await UpdateUnitResultAsync(unitResult, percentModule);
                }
                await UpdateCourseResultAsync(courseResult, unitResult.CourseModuleId.Value, cancellationToken);
            }
            catch
            {
            }
        }

        private async Task UpdateUnitResultAsync(UnitResult unitResult, double percentModule)
        {
            if (unitResult == null)
            {
                return;
            }

            unitResult.PercentModule = NumberHelper.ConvertDoublePercent(unitResult.Percent * percentModule, 2);
            await _unitResultRepository.BulkUpdateList(new List<UnitResult> { unitResult },
            bulk =>
            {
                bulk.ColumnInputExpression = entity => new
                {
                    entity.PercentModule
                };
            });
        }
    }
}
