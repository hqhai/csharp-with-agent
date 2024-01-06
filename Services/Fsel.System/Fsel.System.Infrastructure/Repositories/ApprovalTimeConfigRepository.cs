// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class ApprovalTimeConfigRepository : BaseRepository<ApprovalTimeConfig>, IApprovalTimeConfigRepository
    {
        public ApprovalTimeConfigRepository(SystemDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
