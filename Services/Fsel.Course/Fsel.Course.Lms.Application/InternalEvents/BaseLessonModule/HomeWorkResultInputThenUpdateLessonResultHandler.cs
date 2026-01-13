// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseLessonModule
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeWorkResultInputThenUpdateLessonResultHandler : BaseLessonResultEventHandler, INotificationHandler<EntityChangedEvent<HomeWorkResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public HomeWorkResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository,
            ILessonModuleRepository lessonModuleRepository,
            ILessonModuleCachingService lessonModuleCachingService,
            IClassForumResultRepository classForumResultRepository,
            IHomeWorkResultRepository homeWorkResultRepository,
            IVideoResultRepository videoResultRepository,
            IDocumentResultRepository documentResultRepository,
            ILessonItemInitializerFactory lessonItemInitializerFactory) : base(lessonResultRepository, lessonModuleRepository, lessonModuleCachingService, classForumResultRepository, homeWorkResultRepository, videoResultRepository, documentResultRepository, lessonItemInitializerFactory)
        {
            _lessonResultRepository = lessonResultRepository;
            _lessonModuleRepository = lessonModuleRepository;
            _homeWorkResultRepository = homeWorkResultRepository;
        }

        public async Task Handle(EntityChangedEvent<HomeWorkResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var homeWorkResult = notification.Data;

            try
            {
                var lessonResult = await _lessonResultRepository.GetByIdAsync(homeWorkResult.LessonResultId);
                if (lessonResult == null || !homeWorkResult.LessonModuleId.HasValue || homeWorkResult.Status != EnumResultStatus.Done)
                {
                    return;
                }

                var percentModule = await _lessonModuleRepository.ReadQueryable
                                             .Where(x => x.Id == homeWorkResult.LessonModuleId)
                                             .Select(x => x.Percent)
                                             .FirstOrDefaultAsync(cancellationToken);
                if (lessonResult.Status != EnumResultStatus.Done)
                {
                    await UpdateHomeWorkResultAsync(homeWorkResult, percentModule, cancellationToken);
                }

                await UpdateLessonResultAsync(lessonResult, homeWorkResult.LessonModuleId.Value, cancellationToken);
            }
            catch
            {
            }
        }

        private async Task UpdateHomeWorkResultAsync(HomeWorkResult homeWorkResult, double percentModule, CancellationToken cancellationToken)
        {
            if (homeWorkResult == null)
            {
                return;
            }
            if (!homeWorkResult.CompletionDate.HasValue)
            {
                homeWorkResult.CompletionDate = DateTime.UtcNow;
            }
            homeWorkResult.PercentModule = NumberHelper.ConvertDoublePercent(homeWorkResult.Percent * percentModule, 2);
            await _homeWorkResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
