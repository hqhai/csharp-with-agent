// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Repositories
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Core.Base;
    using AutoMapper;
    public class WheelOfBuffRepository : BaseRepository<WheelOfBuff>, IWheelOfBuffRepository
    {
        public WheelOfBuffRepository(CmsPlanetDefenderDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }


    }
}
