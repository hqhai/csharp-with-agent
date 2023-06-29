// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class CSORepository : BaseIdentityRepository<CSO, User>, ICSORepository
    {
        public CSORepository(UserDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<CSO?> GetIncludeByUserIdAsync(Guid userId)
        {
            try
            {
                return await Queryable
                .Include(x => x.Human)
                .FirstOrDefaultAsync(x => x.Human!.UserId == userId.ToString());
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
