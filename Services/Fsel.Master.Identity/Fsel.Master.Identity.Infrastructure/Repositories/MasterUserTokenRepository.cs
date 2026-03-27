// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Master.Identity.Domain.Entities;
using global::Fsel.Master.Identity.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Master.Identity.Infrastructure.Repositories
{
    public class MasterUserTokenRepository : IMasterUserTokenRepository
    {
        private readonly AuthContext _authContext;
        private readonly UserMasterDBContext _masterDbContext;

        public MasterUserTokenRepository(UserMasterDBContext masterDbContext, AuthContext authContext)
        {
            _masterDbContext = masterDbContext;
            _authContext = authContext;
        }

        public virtual async Task<MasterUserToken?> GetByRefreshTokenAsync(string? refreshToken)
        {
            try
            {
                return await _masterDbContext.MasterUserTokens
                    .Where(x => !x.IsDeleted)
                    .SingleOrDefaultAsync(x => x.RefreshToken == refreshToken)
                    .ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<MasterUserToken?> AddAsync(MasterUserToken? userToken)
        {
            try
            {
                if (userToken == null)
                {
                    return userToken;
                }
                userToken.CreatedDate = DateTime.UtcNow;
                userToken.CreatedUserId = _authContext.CurrentUserId;
                userToken.CreatedFullName = _authContext.CurrentFullName ?? string.Empty;
                userToken = (await _masterDbContext.MasterUserTokens.AddAsync(userToken)).Entity;
                await _masterDbContext.SaveChangesAsync();
                return userToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<MasterUserToken?> RemoveAsync(MasterUserToken? userToken)
        {
            try
            {
                if (userToken == null)
                {
                    return userToken;
                }
                userToken.DeletedDate = DateTime.UtcNow;
                userToken.DeletedUserId = _authContext.CurrentUserId;
                userToken.DeletedFullName = _authContext.CurrentFullName;
                userToken.IsDeleted = true;
                _masterDbContext.MasterUserTokens.Update(userToken);
                await _masterDbContext.SaveChangesAsync();
                return userToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
