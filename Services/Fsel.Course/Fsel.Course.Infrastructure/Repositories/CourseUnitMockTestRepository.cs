using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class CourseUnitMockTestRepository : BaseRepository<CourseUnitMockTest>, ICourseUnitMockTestRepository
    {
        public CourseUnitMockTestRepository(CourseDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<CourseUnitMockTest>> GetListByUnitIdAsync(Guid courseId)
        {
            return await Queryable.Where(a => a.CourseId == courseId).ToListAsync();
        }
    }
}