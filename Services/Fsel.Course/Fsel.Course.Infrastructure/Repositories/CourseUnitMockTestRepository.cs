// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseUnitMockTestRepository : BaseRepository<CourseUnitMockTest>, ICourseUnitMockTestRepository
    {
        public CourseUnitMockTestRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper): base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<List<CourseUnitMockTest>> GetListByUnitIdAsync(Guid courseId)
        {
            return await Queryable.Where(a => a.CourseId == courseId).ToListAsync();
        }
    }
}
