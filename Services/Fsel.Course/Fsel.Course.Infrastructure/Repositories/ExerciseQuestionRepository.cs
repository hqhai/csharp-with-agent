using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class ExerciseQuestionRepository : BaseRepository<ExerciseQuestion>, IExerciseQuestionRepository
    {
        public ExerciseQuestionRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}