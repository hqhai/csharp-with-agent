// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Core.Base.Interfaces;
    using Entities;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAiCriteriaConfigRepository : IRepository<AICriteriaConfigs>
    {
        Task<List<AICriteriaConfigs>> GetAllByAiPromptManagerIdAsync(Guid aiPromptManagerId);
        Task<AICriteriaConfigs?> GetLastVersionByOriginalIdAsync(Guid originalId);
    }
}
