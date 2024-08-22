// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class UserConfigRepository : BaseRepository<UserConfig>, IUserConfigRepository
    {
        public UserConfigRepository(SystemDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
