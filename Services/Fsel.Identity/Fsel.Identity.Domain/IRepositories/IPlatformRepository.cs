// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;

    public interface IPlatformRepository : IRepository<Platform>
    {
        Task<Platform?> GetPlatformAsync(EnumPlatformCode code, CancellationToken cancellationToken);
    }
}
