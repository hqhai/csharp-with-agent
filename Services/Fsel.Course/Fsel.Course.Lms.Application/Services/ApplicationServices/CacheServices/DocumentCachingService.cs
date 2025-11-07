// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices
{
    using Common.Caching;
    using Domain.Entities;

    public interface IDocumentCachingService : IEntityCachingService<Document>
    {

    }

    public class DocumentCachingService : EntityCachingService<Document>, IDocumentCachingService
    {
        public DocumentCachingService(ICacheService<Document> cacheService) : base(cacheService)
        {
        }

        public override List<string> Tags => new List<string> { "DocumentService", "Entity", nameof(Document)};

        public override string Prefix => $"DocumentService:Entity:{nameof(Document)}";
    }
}
