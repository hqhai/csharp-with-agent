// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using global::Fsel.Master.Identity.Domain.Entities;
using global::Fsel.Master.Identity.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Fsel.Master.Identity.Infrastructure.Repositories
{
    public class UserEventRepository : BaseRepository<UserEvent>, IUserEventRepository
    {
        public UserEventRepository(UserMasterDBContext dbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, authContext, mapper)
        {
        }

        public async Task<UserEvent?> GetLatestByUserIdAsync(Guid userId)
        {
            return await Queryable
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();
        }
    }
}
