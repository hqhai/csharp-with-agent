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
    using Microsoft.EntityFrameworkCore;

    public class ClassForumResultInputThenUpdateLessonResultHandler : BaseLessonResultEventHandler, INotificationHandler<EntityChangedEvent<ClassForumResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IClassForumResultRepository _classForumResultRepository;

        public ClassForumResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository,
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
            _classForumResultRepository = classForumResultRepository;
        }

        public async Task Handle(EntityChangedEvent<ClassForumResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var classForumResult = notification.Data;
            try
            {
                var lessonResult = await _lessonResultRepository.GetByIdAsync(classForumResult.LessonResultId);
                if (lessonResult == null || !classForumResult.LessonModuleId.HasValue || classForumResult.ResultStatus != EnumResultStatus.Done)
                {
                    return;
                }
                var percentModule = await _lessonModuleRepository.ReadQueryable
                                          .Where(x => x.Id == classForumResult.LessonModuleId)
                                          .Select(x => x.Percent)
                                          .FirstOrDefaultAsync(cancellationToken);
                if (lessonResult.Status != EnumResultStatus.Done)
                {
                    await UpdateClassForumResultAsync(classForumResult, percentModule, cancellationToken);
                }

                await UpdateLessonResultAsync(lessonResult, classForumResult.LessonModuleId.Value, cancellationToken);
            }
            catch
            {
            }
        }

        private async Task UpdateClassForumResultAsync(ClassForumResult classForumResult, double percentModule, CancellationToken cancellationToken)
        {
            if (classForumResult == null)
            {
                return;
            }

            classForumResult.PercentModule = NumberHelper.ConvertDoublePercent(classForumResult.Percent * percentModule, 2);
            await _classForumResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
