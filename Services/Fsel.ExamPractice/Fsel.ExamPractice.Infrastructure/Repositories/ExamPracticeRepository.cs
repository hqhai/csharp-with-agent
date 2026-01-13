namespace Fsel.ExamPractice.Infrastructure.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class ExamPracticeRepository : BaseRepository<ExamPractice>, IExamPracticeRepository
    {
        public ExamPracticeRepository(ExamPracticesDBContext dbContext, ExamPracticesReadDBContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<bool> IsUsingByClient(Guid id)
        {
            return await DbContext.Set<ExamPracticeResult>().AsNoTracking().AnyAsync(x => x.ExamPracticeId == id);
        }
    }
}
