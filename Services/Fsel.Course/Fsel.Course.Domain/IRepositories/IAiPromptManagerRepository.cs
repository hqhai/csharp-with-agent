// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Core.Base.Interfaces;
    using Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAiPromptManagerRepository : IRepository<AiPromptManager>
    {
        Task<AiPromptManager?> GetLastVersionByOriginalIdAsync(Guid originalId);
        Task<List<AiPromptManager>> GetAllVersionsByOriginalIdAsync(Guid originalId);
        Task<List<AiPromptManager>> GetVersionsByProjectIdAsync(Guid projectId);
        Task MarkAsOldVersionAsync(Guid id);
        Task<int> GetNextVersionNumberAsync(Guid originalId);
    }
}
