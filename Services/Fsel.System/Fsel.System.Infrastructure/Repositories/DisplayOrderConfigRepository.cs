// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class DisplayOrderConfigRepository : BaseRepository<DisplayOrderConfig>, IDisplayOrderConfigRepository
    {
        public DisplayOrderConfigRepository(SystemDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
