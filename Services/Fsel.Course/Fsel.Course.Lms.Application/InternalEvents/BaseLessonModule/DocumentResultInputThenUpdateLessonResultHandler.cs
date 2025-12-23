// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.InternalEvents.BaseLessonModule
{
    using Fsel.Core.Applications.InternalEvents;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class DocumentResultInputThenUpdateLessonResultHandler : BaseLessonResultEventHandler, INotificationHandler<EntityChangedEvent<DocumentResult>>
    {
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly ILessonModuleRepository _lessonModuleRepository;
        private readonly IDocumentResultRepository _documentResultRepository;

        public DocumentResultInputThenUpdateLessonResultHandler(ILessonResultRepository lessonResultRepository,
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
            _documentResultRepository = documentResultRepository;
        }

        public async Task Handle(EntityChangedEvent<DocumentResult> notification, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(notification);
            var documentResult = notification.Data;
            try
            {
                var lessonResult = await _lessonResultRepository.GetByIdAsync(documentResult.LessonResultId);
                if (lessonResult == null || documentResult.Status != EnumResultStatus.Done)
                {
                    return;
                }
                var percentModule = await _lessonModuleRepository.ReadQueryable
                                       .Where(x => x.Id == documentResult.LessonModuleId)
                                       .Select(x => x.Percent)
                                       .FirstOrDefaultAsync(cancellationToken);
                if (lessonResult.Status != EnumResultStatus.Done)
                {
                    await UpdateResultAsync(documentResult, percentModule);
                }

                await UpdateLessonResultAsync(lessonResult, documentResult.LessonModuleId, cancellationToken);
            }
            catch
            {
            }
        }

        private async Task UpdateResultAsync(DocumentResult documentResult, double percentModule)
        {
            if (documentResult == null)
            {
                return;
            }

            documentResult.PercentModule = percentModule;
            await _documentResultRepository.BulkUpdateList(new List<DocumentResult> { documentResult },
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
