// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Repositories
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Core.Base;

    public class ZMatterRepository : BaseRepository<ZMatter>, IZMatterRepository
    {
        public ZMatterRepository(CmsPlanetDefenderDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
