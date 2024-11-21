// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class PlatformRepository : BaseRepository<Platform>, IPlatformRepository
    {
        private readonly ILogger<PlatformRepository> _logger;
        public PlatformRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper, ILogger<PlatformRepository> logger) : base(dbContext, authContext, mapper)
        {
            _logger = logger;
        }

        public async Task<Platform?> GetPlatformAsync(EnumPlatformCode code, CancellationToken cancellationToken)
        {
            try
            {
                return await Queryable.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PlatformRepository.GetPlatformAsync encouters error: {message}", ex.Message);
                throw;
            }
        }
    }
}
