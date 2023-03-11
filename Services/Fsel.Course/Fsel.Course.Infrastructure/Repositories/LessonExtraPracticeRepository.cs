using Fsel.Core.Base;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.IRepositories;

namespace Fsel.Course.Infrastructure.Repositories
{
    public class LessonExtraPracticeRepository : BaseRepository<LessonExtraPractice>, ILessonExtraPracticeRepository
    {
        public LessonExtraPracticeRepository(CourseDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}