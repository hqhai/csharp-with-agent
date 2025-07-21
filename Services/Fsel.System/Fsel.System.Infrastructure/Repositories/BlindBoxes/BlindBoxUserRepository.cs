using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fsel.Core.Base;
using Fsel.System.Domain.Entities;
using Fsel.System.Domain.Entities.BlindBoxs;
using Fsel.System.Domain.IRepositories;
using Fsel.System.Domain.IRepositories.BlindBoxes;

namespace Fsel.System.Infrastructure.Repositories.BlindBoxes
{
    public class BlindBoxUserRepository : BaseRepository<BlindBoxUser>, IBlindBoxUserRepository
    {
        public BlindBoxUserRepository(SystemDbContext dbContext, SystemReadDbContext readDbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }
    }
}
