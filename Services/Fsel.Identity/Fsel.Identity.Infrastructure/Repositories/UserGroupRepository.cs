// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class UserGroupRepository : BaseRepository<UserGroup>, IUserGroupRepository
    {
        public UserGroupRepository(UserDbContext dbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, authContext, mapper)
        {
        }
    }
}
