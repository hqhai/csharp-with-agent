// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.ApplicationServices.CacheServices;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class DocumentLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentResultRepository _documentResultRepository;
        private readonly IRequestSafeCachingService _requestSafeCachingService;
        private readonly ILogger<DocumentLessonItemInitializer> _logger;

        public DocumentLessonItemInitializer(
            IDocumentRepository documentRepository,
            IDocumentResultRepository documentResultRepository,
            IRequestSafeCachingService requestSafeCachingService,
            ILogger<DocumentLessonItemInitializer> logger)
        {
            _documentRepository = documentRepository;
            _documentResultRepository = documentResultRepository;
            _requestSafeCachingService = requestSafeCachingService;
            _logger = logger;
        }

        public async Task<VoidMethodResult> InitializeAsync(
            LessonModule lessonModule,
            LessonResult lessonResult,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);

            var methodResult = new VoidMethodResult();

            try
            {
                if (lessonModule.LessonConfigType != EnumLessonConfigType.Document)
                {
                    _logger.LogDebug(
                        "Skip Document initialization because LessonConfigType is not Document. LessonModuleId={LessonModuleId}, LessonResultId={LessonResultId}, ActualType={LessonConfigType}",
                        lessonModule.Id,
                        lessonResult.Id,
                        lessonModule.LessonConfigType);

                    return methodResult;
                }

                var documentResult = await _documentResultRepository.Queryable
                    .Where(x => x.LessonResultId == lessonResult.Id)
                    .Where(x => x.LessonModuleId == lessonModule.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (documentResult != null)
                {
                    if (documentResult.Status == EnumResultStatus.Unfinished)
                    {
                        _logger.LogInformation(
                            "Found existing DocumentResult in Unfinished status. Resetting to New. DocumentResultId={DocumentResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, StudentId={StudentId}",
                            documentResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            lessonResult.StudentId);

                        documentResult.Status = EnumResultStatus.New;
                        documentResult.NewDate = DateTime.UtcNow;

                        await _documentResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "DocumentResult already exists, no initialization needed. DocumentResultId={DocumentResultId}, LessonResultId={LessonResultId}, LessonModuleId={LessonModuleId}, Status={Status}",
                            documentResult.Id,
                            lessonResult.Id,
                            lessonModule.Id,
                            documentResult.Status);
                    }

                    return methodResult;
                }

                var document = await _documentRepository.ReadQueryable
                    .Where(x => x.OriginalId == lessonModule.OriginalId)
                    .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                    .FirstOrDefaultAsync(cancellationToken);

                if (document == null)
                {
                    _logger.LogWarning(
                        "Document not found for LessonModule OriginalId. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                        lessonModule.Id,
                        lessonModule.OriginalId,
                        lessonResult.Id,
                        lessonResult.StudentId);

                    methodResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(document),
                        lessonModule.OriginalId);

                    return methodResult;
                }

                documentResult = new DocumentResult
                {
                    LessonModuleId = lessonModule.Id,
                    LessonResultId = lessonResult.Id,
                    StudentId = lessonResult.StudentId,
                    Status = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    DocumentId = document.Id,
                };

                await _requestSafeCachingService.SafeRequest(
                    key: $"Add_DocumentResult_{documentResult.LessonModuleId}_{documentResult.LessonResultId}_{documentResult.IsDeleted}",
                    safeFunction: async () =>
                    {
                        await _documentResultRepository.BulkMergeAsync(
                            new List<DocumentResult> { documentResult },
                            bulk =>
                            {
                                bulk.ColumnPrimaryKeyExpression = c => new
                                {
                                    c.LessonModuleId,
                                    c.LessonResultId,
                                    c.IsDeleted
                                };
                            });

                        return documentResult;
                    });

                return methodResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while initializing Document lesson item. LessonModuleId={LessonModuleId}, LessonModuleOriginalId={LessonModuleOriginalId}, LessonResultId={LessonResultId}, StudentId={StudentId}",
                    lessonModule.Id,
                    lessonModule.OriginalId,
                    lessonResult.Id,
                    lessonResult.StudentId);

                throw;
            }
        }
    }
}
