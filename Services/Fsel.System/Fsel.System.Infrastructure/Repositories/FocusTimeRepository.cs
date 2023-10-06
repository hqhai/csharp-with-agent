// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class FocusTimeRepository : BaseRepository<FocusTimeConfig>, IFocusTimeConfigRepository
    {
        public FocusTimeRepository(SystemDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
