// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.V1i1;

    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IDictionary<Guid, Document>> GetDocumentDicAsync(IList<Guid>? originalIds);

        Task<(IDictionary<Guid, (Document, DocumentResult)>, IDictionary<Guid, Document>)> BuildDocumentLookupsAsync(LessonResult lessonResult, IList<LessonModule> lessonModules);
    }
}
