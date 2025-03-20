using AutoMapper;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities.BlindBoxs;
using Fsel.System.Domain.IRepositories.BlindBoxes;

namespace Fsel.System.Infrastructure.Repositories.BlindBoxes
{
    public class BlindBoxRepository : BaseRepository<BlindBox>, IBlindBoxRepository
    {
        public BlindBoxRepository(SystemDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
