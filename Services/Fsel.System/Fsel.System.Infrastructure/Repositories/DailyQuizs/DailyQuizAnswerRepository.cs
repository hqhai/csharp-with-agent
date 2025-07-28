using AutoMapper;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities.DailyQuiz;
using Fsel.System.Domain.IRepositories.DailyQuizs;

namespace Fsel.System.Infrastructure.Repositories.DailyQuizs
{
    public class DailyQuizAnswerRepository : BaseRepository<DailyQuizAnswer>, IDailyQuizAnswerRepository
    {
        public DailyQuizAnswerRepository(SystemDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public override IQueryable<DailyQuizAnswer> Queryable => _dbSet.AsQueryable();
    }
}
