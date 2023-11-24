// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Core.Base;

    public class GameAnswerRepository : BaseRepository<GameAnswer>, IGameAnswerRepository
    {
        public GameAnswerRepository(CmsPlanetDefenderDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
