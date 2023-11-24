// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Infrastructure.Repositories
{
    using Fsel.Cms.PlanetDefender.Domain.Entities;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Core.Base;

    public class StudentTagNameRepository : BaseRepository<StudentTagName>, IStudentTagNameRepository
    {
        public StudentTagNameRepository(CmsPlanetDefenderDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
