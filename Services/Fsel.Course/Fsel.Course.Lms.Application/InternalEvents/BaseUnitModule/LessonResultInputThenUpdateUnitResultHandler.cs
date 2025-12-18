// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class LessonResultInputThenUpdateUnitResultHandler : BaseUnitResultEventHandler, INotificationHandler<EntityChangedEvent<LessonResult>>
    {
        private readonly IUnitModuleRepository _unitModuleRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;

        public LessonResultInputThenUpdateUnitResultHandler(IUnitModuleCachingService unitModuleCachingService,
            IUnitModuleRepository unitModuleRepository,
            IUnitResultRepository unitResultRepository,
            ILessonResultRepository lessonResultRepository,
            ITestGroupResultRepository testGroupResultRepository,
            IUnitItemInitializerFactory unitItemInitializerFactory) : base(unitModuleCachingService, unitModuleRepository, unitResultRepository, lessonResultRepository, testGroupResultRepository, unitItemInitializerFactory)
        {
            _unitModuleRepository = unitModuleRepository;
            _unitResultRepository = unitResultRepository;
            _lessonResultRepository = lessonResultRepository;
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
                if (unitResult == null || !lessonResult.UnitModuleId.HasValue || lessonResult.Status != EnumResultStatus.Done)
                {
                    return;
                }

                var percentModule = await _unitModuleRepository.ReadQueryable
                                          .Where(x => x.Id == lessonResult.UnitModuleId)
                                          .Select(x => x.Percent)
                                          .FirstOrDefaultAsync(cancellationToken);
                await UpdateLessonResultAsync(lessonResult, percentModule);
                await UpdateUnitResultAsync(unitResult, lessonResult.UnitModuleId.Value, cancellationToken);
            }
            catch
            {
            }
        }

        private async Task UpdateLessonResultAsync(LessonResult lessonResult, double percentModule)
        {
            if (lessonResult == null)
            {
                return;
            }

            lessonResult.PercentModule = NumberHelper.ConvertDoublePercent(lessonResult.Percent * percentModule, 2);
            await _lessonResultRepository.BulkUpdateList(new List<LessonResult> { lessonResult },
            bulk =>
            {
                bulk.ColumnInputExpression = entity => new
                {
                    entity.PercentModule
                };
            }).ConfigureAwait(false);
        }
    }
}
