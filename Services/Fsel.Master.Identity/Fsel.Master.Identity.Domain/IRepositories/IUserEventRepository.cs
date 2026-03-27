// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.Interfaces;
using global::Fsel.Master.Identity.Domain.Entities;

namespace Fsel.Master.Identity.Domain.IRepositories
{
    public interface IUserEventRepository : IRepository<UserEvent>
    {
        Task<UserEvent?> GetLatestByUserIdAsync(Guid userId);
    }
}

