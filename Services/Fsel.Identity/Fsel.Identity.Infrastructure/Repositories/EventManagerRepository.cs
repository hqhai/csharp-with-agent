// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;

    public class EventManagerRepository : BaseRepository<EventManager>, IEventManagerRepository
    {
        public EventManagerRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper)
            : base(dbContext, authContext, mapper)
        {
        }
    }
}
