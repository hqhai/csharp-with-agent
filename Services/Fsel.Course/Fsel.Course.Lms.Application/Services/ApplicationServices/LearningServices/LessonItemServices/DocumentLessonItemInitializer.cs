// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices
{
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class DocumentLessonItemInitializer : ILessonItemInitializer
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentResultRepository _documentResultRepository;

        public DocumentLessonItemInitializer(IDocumentRepository documentRepository, IDocumentResultRepository documentResultRepository)
        {
            _documentRepository = documentRepository;
            _documentResultRepository = documentResultRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(LessonModule lessonModule, LessonResult lessonResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(lessonModule);
            ArgumentNullException.ThrowIfNull(lessonResult);
            var methodResult = new VoidMethodResult();
            if (lessonModule.LessonConfigType != EnumLessonConfigType.Document)
            {
                return methodResult;
            }

            var document = await _documentRepository.ReadQueryable.Where(x => x.OriginalId == lessonModule.OriginalId)
                                                .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                                .FirstOrDefaultAsync(cancellationToken);

            if (document == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(document), lessonModule.OriginalId);
                return methodResult;
            }

            var documentResult = await _documentResultRepository.ReadQueryable.Where(x => x.LessonResultId == lessonResult.Id)
                                                          .Where(x => x.LessonModuleId == lessonModule.Id)
                                                          .FirstOrDefaultAsync(cancellationToken);
            if (documentResult != null)
            {
                return methodResult;
            }

            documentResult = new DocumentResult
            {
                LessonModuleId = lessonModule.Id,
                LessonResultId = lessonResult.Id,
                StudentId = lessonResult.StudentId,
                Status = EnumResultStatus.New,
                DocumentId = document.Id,
            };

            try
            {
                await _documentResultRepository.BulkMergeAsync(new List<DocumentResult> { documentResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.LessonModuleId, c.LessonResultId, c.IsDeleted };
                });
            }
            catch { }
            return methodResult;
        }
    }
}
