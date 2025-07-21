namespace Fsel.ExamPractice.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Entities;

    public class ExamPracticeRepository : BaseRepository<ExamPractice>, IExamPracticeRepository
    {
        public ExamPracticeRepository(ExamPracticesDBContext dbContext, ExamPracticesReadDBContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
