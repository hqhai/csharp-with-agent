// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class PlatformRepository : BaseRepository<Platform>, IPlatformRepository
    {
        public PlatformRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<Platform?> GetPlatformAsync(EnumPlatformCode code, CancellationToken cancellationToken)
        {
            try
            {
                return await Queryable.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
