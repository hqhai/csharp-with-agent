// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities.Campus;
    using Fsel.Identity.Domain.IRepositories;

    public class SchoolClassRepository : BaseRepository<SchoolClass>, ISchoolClassRepository
    {
        public SchoolClassRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
