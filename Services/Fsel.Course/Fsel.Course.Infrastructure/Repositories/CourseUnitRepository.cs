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
    public class CourseUnitRepository : BaseRepository<CourseUnit>, ICourseUnitRepository
    {
        public CourseUnitRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }

        public async Task<List<CourseUnit>> GetListByUnitIdAsync(Guid courseId)
        {
            return await Queryable.Where(a => a.CourseId == courseId).ToListAsync();
        }
    }
}