// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitLessonRepository : BaseRepository<UnitLesson>, IUnitLessonRepository
    {
        public UnitLessonRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<List<UnitLesson>> GetListByUnitIdAsync(Guid unitId)
        {
            return await Queryable.Where(a => a.UnitId == unitId).ToListAsync();
        }
    }
}
