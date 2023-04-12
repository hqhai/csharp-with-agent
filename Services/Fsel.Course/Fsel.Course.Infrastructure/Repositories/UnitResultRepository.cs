// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitResultRepository : BaseRepository<UnitResult>, IUnitResultRepository
    {
        public UnitResultRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
