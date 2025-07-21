using AutoMapper;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities.DailyQuiz;
using Fsel.System.Domain.IRepositories.DailyQuizs;

namespace Fsel.System.Infrastructure.Repositories.DailyQuizs
{
    public class DailyQuizQuestionRepository : BaseRepository<DailyQuizQuestion>, IDailyQuizQuestionRepository
    {
        public DailyQuizQuestionRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public override IQueryable<DailyQuizQuestion> Queryable => _dbSet.AsQueryable();
    }
}
