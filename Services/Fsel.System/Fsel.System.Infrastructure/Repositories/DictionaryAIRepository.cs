// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using global::System;
    using global::System.Collections.Generic;
    using global::System.Threading;
    using global::System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using Npgsql;

#pragma warning disable EF1001 // Internal EF Core API usage

    /// <summary>
    /// Repository implementation cho DictionaryAI với pgvector semantic search
    /// </summary>
    public class DictionaryAIRepository : BaseRepository<DictionaryAI>, IDictionaryAIRepository
    {
        private readonly PostgreDbContext _context;

        public DictionaryAIRepository(PostgreDbContext dbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, authContext, mapper)
        {
            _context = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DictionaryAI>> VectorSearchAsync(
            float[] embedding,
            string sourceLanguage,
            string targetLanguage,
            string normalizedItem,
            double threshold = 0.85,
            int limit = 5,
            CancellationToken cancellationToken = default)
        {
            var embeddingString = "[" + string.Join(",", embedding.Select(x => x.ToString("G9"))) + "]";

            var sql = @"
                SELECT d.""Id"", d.""HighlightedItemSource"", d.""HighlightedItemTarget"",
                       d.""DefinitionSource"", d.""DefinitionTarget"", d.""JsonPayload"",
                       d.""NotesJson"", d.""ExampleSentenceSource"", d.""ExampleSentenceTarget"",
                       d.""SourceLanguage"", d.""TargetLanguage"", d.""HasAudioFromLegacy"",
                       d.""ModelName"", d.""InputLength"", d.""Embedding"",
                       d.""CreatedUserId"", d.""UpdatedUserId"", d.""DeletedUserId"",
                       d.""CreatedFullName"", d.""UpdatedFullName"", d.""DeletedFullName"",
                       d.""CreatedDate"", d.""UpdatedDate"", d.""DeletedDate"", d.""IsDeleted"",
                       1 - (d.""Embedding""::vector <=> @p3::vector) as ""Similarity""
                FROM ""DictionaryAIs"" d
                WHERE d.""SourceLanguage"" = @p0
                  AND d.""TargetLanguage"" = @p1
                  AND d.""IsDeleted"" = false
                  AND (
                    LOWER(d.""HighlightedItemSource"") = @p2
                    OR LOWER(d.""HighlightedItemSource"") LIKE @p2 || '%'
                  )
                  AND d.""Embedding"" IS NOT NULL
                  AND (d.""Embedding""::vector <=> @p3::vector) < @p4
                ORDER BY ""Similarity"" DESC
                LIMIT @p5";

            var results = await _context.Set<DictionaryAI>()
                .FromSqlRaw(sql, sourceLanguage, targetLanguage, normalizedItem, embeddingString, threshold, limit)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return results;
        }
    }
}
