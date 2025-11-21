// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.Enums;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class DocumentRepository : BaseRepository<Document>, IDocumentRepository
    {
        private readonly IDocumentResultRepository _documentResultRepository;

        public DocumentRepository(CourseDbContext dbContext,
            CourseReadDbContext readDbContext,
            AuthContext authContext,
            IMapper mapper,
            IDocumentResultRepository documentResultRepository)
            : base(dbContext, readDbContext, authContext, mapper)
        {
            _documentResultRepository = documentResultRepository;
        }

        public async Task<IDictionary<Guid, Document>> GetDocumentDicAsync(IList<Guid>? originalIds)
        {
            if (originalIds == null || originalIds.Count == 0)
            {
                return new Dictionary<Guid, Document>();
            }

            var documents = await ReadQueryable.WhereBulkContains(originalIds, x => x.OriginalId)
                                               .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                               .ToListAsync();

            return documents.ToDictionary(x => x.OriginalId);
        }

        public async Task<(IDictionary<Guid, (Document, DocumentResult)>, IDictionary<Guid, Document>)>
        BuildDocumentLookupsAsync(LessonResult lessonResult, IList<LessonModule> lessonModules)
        {
            var documentOriginalIds = lessonModules
                .Where(x => x.LessonConfigType == EnumLessonConfigType.Document)
                .Select(x => x.OriginalId)
                .Distinct()
                .ToList();

            if (!documentOriginalIds.Any())
            {
                return (new Dictionary<Guid, (Document, DocumentResult)>(),
                        new Dictionary<Guid, Document>());
            }

            var documentResults = await (from baseQ in _documentResultRepository.ReadQueryable
                                         where baseQ.LessonResultId == lessonResult.Id
                                         join document in ReadQueryable
                                             on baseQ.DocumentId equals document.Id
                                         select new
                                         {
                                             Document = document,
                                             DocumentResult = baseQ
                                         }).ToListAsync();

            var documentOriginalIdsHasResult = documentResults
                .Where(x => x.Document != null)
                .Select(x => x.Document!.OriginalId)
                .Distinct();

            var pendingDocumentOriginalIds = documentOriginalIds
                .Except(documentOriginalIdsHasResult)
                .ToList();

            var documentDics = await GetDocumentDicAsync(pendingDocumentOriginalIds);

            var documentResultsByOriginalId = documentResults
                .Where(x => x.Document != null)
                .ToDictionary(
                    x => x.Document!.OriginalId,
                    x => (Document: x.Document!, DocumentResult: x.DocumentResult));

            return (documentResultsByOriginalId, documentDics);
        }
    }
}
