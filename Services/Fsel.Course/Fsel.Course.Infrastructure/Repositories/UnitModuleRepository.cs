// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.IRepositories;

    public class UnitModuleRepository : BaseRepository<UnitModule>, IUnitModuleRepository
    {
        public UnitModuleRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
