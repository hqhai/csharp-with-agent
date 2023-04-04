// Copyright (c) Atlantic. All rights reserved.

using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly UserDbContext _userDbContext;

        public UserTokenRepository(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        public virtual async Task<UserToken?> GetByRefreshTokenAsync(string? refreshToken)
        {
            try
            {
                return await _userDbContext.UserTokens.SingleOrDefaultAsync(x => x.RefreshToken == refreshToken).ConfigureAwait(continueOnCapturedContext: false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<UserToken> AddAsync(UserToken userToken)
        {
            try
            {
                userToken = (await _userDbContext.UserTokens.AddAsync(userToken)).Entity;
                await _userDbContext.SaveChangesAsync();
                return userToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<UserToken> Remove(UserToken userToken)
        {
            try
            {
                _userDbContext.UserTokens.Remove(userToken);
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
