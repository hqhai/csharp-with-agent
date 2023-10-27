// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;

    public interface ITeacherRepository : IRepository<Teacher>
    {
        Task<IList<Teacher>> GetIncludeByIdsAsync(IList<Guid> ids, int? siteId = null);

        Task<Teacher?> GetIncludeByUserIdAsync(Guid userId, int? siteId = null);
    }
}
