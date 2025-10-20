// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IRepositories
{
    using Core.Base.Interfaces;
    using Entities;

    public interface IAiPromptManagerRepository : IRepository<AiPromptManager>
    {
        Task<bool> IsFeature(Guid id);
    }
}
