using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class LessonHomeWorkRepository : BaseRepository<LessonHomeWork>, ILessonHomeWorkRepository
    {
        public LessonHomeWorkRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}