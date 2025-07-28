using AutoMapper;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities.DailyQuiz;
using Fsel.System.Domain.IRepositories.DailyQuizs;

namespace Fsel.System.Infrastructure.Repositories.DailyQuizs
{
    public class DailyQuizWinnerRepository : BaseRepository<DailyQuizWinner>, IDailyQuizWinnerRepository
    {
        public DailyQuizWinnerRepository(SystemDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
