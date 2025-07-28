using AutoMapper;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities.BlindBoxs;
using Fsel.System.Domain.IRepositories.BlindBoxes;

namespace Fsel.System.Infrastructure.Repositories.BlindBoxes
{
    public class BlindBoxChestConfigRepository : BaseRepository<BlindBoxChestConfig>, IBlindBoxChestConfigRepository
    {
        public BlindBoxChestConfigRepository(SystemDbContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
