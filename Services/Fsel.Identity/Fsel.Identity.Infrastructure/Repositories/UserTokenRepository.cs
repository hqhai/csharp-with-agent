// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly AuthContext _authContext;
        private readonly UserDbContext _userDbContext;

        public UserTokenRepository(UserDbContext userDbContext, AuthContext authContext)
        {
            _userDbContext = userDbContext;
            _authContext = authContext;
        }

        public virtual async Task<UserToken?> GetByRefreshTokenAsync(string? refreshToken)
        {
            try
            {
                return await _userDbContext.UserTokens.Where(x => !x.IsDeleted).SingleOrDefaultAsync(x => x.RefreshToken == refreshToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<UserToken?> AddAsync(UserToken? userToken)
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
                userToken = (await _userDbContext.UserTokens.AddAsync(userToken)).Entity;
                await _userDbContext.SaveChangesAsync();
                return userToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<UserToken?> Remove(UserToken? userToken)
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
                _userDbContext.UserTokens.Update(userToken);
                await _userDbContext.SaveChangesAsync();
                return userToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
