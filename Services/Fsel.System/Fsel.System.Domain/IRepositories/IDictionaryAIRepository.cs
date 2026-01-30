// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.System.Domain.Entities;

    /// <summary>
    /// Repository interface cho DictionaryAI entity với pgvector support
    /// </summary>
    public interface IDictionaryAIRepository : IRepository<DictionaryAI>
    {
        /// <summary>
        /// Vector search với cosine similarity (pgvector)
        /// </summary>
        Task<IEnumerable<DictionaryAI>> VectorSearchAsync(
            float[] embedding,
            string sourceLanguage,
            string targetLanguage,
            string normalizedItem,
            double threshold = 0.85,
            int limit = 5,
            CancellationToken cancellationToken = default);
    }
}
