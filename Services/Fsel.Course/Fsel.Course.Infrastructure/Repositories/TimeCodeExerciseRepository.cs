using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class TimeCodeExerciseRepository : BaseRepository<TimeCodeExercise>, ITimeCodeExerciseRepository
    {
        public TimeCodeExerciseRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}