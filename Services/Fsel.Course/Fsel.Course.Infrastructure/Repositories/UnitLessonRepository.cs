using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class UnitLessonRepository : BaseRepository<UnitLesson>, IUnitLessonRepository
    {
        public UnitLessonRepository(CourseDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<UnitLesson>> GetListByUnitIdAsync(Guid unitId)
        {
            return await Queryable.Where(a => a.UnitId == unitId).ToListAsync();
        }
    }
}