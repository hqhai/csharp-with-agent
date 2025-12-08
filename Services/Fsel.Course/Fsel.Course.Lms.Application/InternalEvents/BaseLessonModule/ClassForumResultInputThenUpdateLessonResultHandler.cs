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
    using MediatR;

    public class ClassForumResultInputThenUpdateLessonResultHandler : BaseLessonResultEventHandler, INotificationHandler<EntityChangedEvent<ClassForumResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;

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

                await UpdateLessonResultAsync(lessonResult, classForumResult.LessonModuleId.Value, cancellationToken);
            }
            catch
            {
            }
        }
    }
}
