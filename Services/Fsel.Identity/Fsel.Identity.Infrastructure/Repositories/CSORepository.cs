// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class CSORepository : BaseRepository<CSO>, ICSORepository
    {
        public CSORepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<CSO?> GetIncludeByUserIdAsync(Guid userId)
        {
            try
            {
                return await Queryable
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.UserId == userId);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
