using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class TimeCodeExcerciseRepository : BaseRepository<TimeCodeExcercise>, ITimeCodeExcerciseRepository
    {
        public TimeCodeExcerciseRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}