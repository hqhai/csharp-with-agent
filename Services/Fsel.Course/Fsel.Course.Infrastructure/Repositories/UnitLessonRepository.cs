// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitLessonRepository : BaseRepository<UnitLesson>, IUnitLessonRepository
    {
        public UnitLessonRepository(CourseDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<List<UnitLesson>> GetListByUnitIdAsync(Guid unitId)
        {
            return await Queryable.Where(a => a.UnitId == unitId).ToListAsync();
        }
    }
}
