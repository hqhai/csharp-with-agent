// Copyright (c) Atlantic. All rights reserved.

using Fsel.Master.Identity.Domain.Entities;

namespace Fsel.Master.Identity.Domain.IRepositories
{
    public interface IMasterUserTokenRepository
    {
        Task<MasterUserToken?> GetByRefreshTokenAsync(string? refreshToken);

        Task<MasterUserToken?> AddAsync(MasterUserToken? userToken);

        Task<MasterUserToken?> RemoveAsync(MasterUserToken? userToken);
    }
}
