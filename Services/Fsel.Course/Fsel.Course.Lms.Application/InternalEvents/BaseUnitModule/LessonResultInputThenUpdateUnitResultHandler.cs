// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices;
    using MediatR;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseUnitResultEventHandler, INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly IUnitResultRepository _unitResultRepository;

        public LessonResultInputThenUpdateUnitResultHandler(IUnitModuleCachingService unitModuleCachingService,
            IUnitModuleRepository unitModuleRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ITestRepository testRepository,
            ITestGroupResultRepository testGroupResultRepository,
            IUnitItemInitializerFactory unitItemInitializerFactory) : base(unitModuleCachingService, unitModuleRepository, unitResultRepository, lessonResultRepository, testRepository, testGroupResultRepository, unitItemInitializerFactory)
        {
            _unitResultRepository = unitResultRepository;
        }

        public async Task Handle(EntityChangedEvent<LessonResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var lessonResult = notification.Data;

            if (!lessonResult.UnitResultId.HasValue)
            {
                return;
            }

            try
            {
                var unitResult = await _unitResultRepository.GetByIdAsync(lessonResult.UnitResultId.Value);
                if (unitResult == null || lessonResult.Status != EnumResultStatus.Done)
                {
                    return;
                }

                var unitModules = await GetUnitModulesAsync(lessonResult.UnitId);

                await UpdateUnitResultAsync();
            }
            catch
            {
            }
        }
    }
}
