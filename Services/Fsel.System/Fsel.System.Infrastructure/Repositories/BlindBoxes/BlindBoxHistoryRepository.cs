using AutoMapper;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities.BlindBoxs;
using Fsel.System.Domain.IRepositories.BlindBoxes;

namespace Fsel.System.Infrastructure.Repositories.BlindBoxes
{
    public class BlindBoxHistoryRepository : BaseRepository<BlindBoxHistory>, IBlindBoxHistoryRepository
    {
        public BlindBoxHistoryRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
