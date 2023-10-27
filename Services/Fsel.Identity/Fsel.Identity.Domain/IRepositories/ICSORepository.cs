// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;

    public interface ICSORepository : IRepository<CSO>
    {
        Task<CSO?> GetIncludeByUserIdAsync(Guid userId);
    }
}
