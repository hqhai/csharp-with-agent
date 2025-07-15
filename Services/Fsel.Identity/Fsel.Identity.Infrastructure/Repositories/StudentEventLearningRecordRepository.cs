using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class StudentEventLearningRecordRepository : BaseRepository<StudentEventLearningRecord>, IStudentEventLearningRecordRepository
    {
        public StudentEventLearningRecordRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
