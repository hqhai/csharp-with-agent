// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class ForbiddenWordRepository : BaseRepository<ForbiddenWord>, IForbiddenWordRepository
    {
        public ForbiddenWordRepository(SystemDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
