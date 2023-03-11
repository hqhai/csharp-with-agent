using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class ExcerciseQuestionRepository : BaseRepository<ExcerciseQuestion>, IExcerciseQuestionRepository
    {
        public ExcerciseQuestionRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}