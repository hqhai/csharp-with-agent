// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using AutoMapper;

    public class ReferralDiscountConfigRepository : BaseRepository<ReferralDiscountConfig>, IReferralDiscountConfigRepository
    {
        public ReferralDiscountConfigRepository(SystemDbContext dbContext, SystemReadDbContext systemReadDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, systemReadDbContext, authContext, mapper)
        {
        }
    }
}
