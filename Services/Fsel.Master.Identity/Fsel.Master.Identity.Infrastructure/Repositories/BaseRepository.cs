// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Identity.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Core.Entities;
    using global::Fsel.Master.Identity.Domain.Entities;

    public class BaseRepository<T> : BaseIdentityRepository<T, MasterUser, MasterRole, Guid, MasterUserClaim, MasterUserRole, MasterUserLogin, MasterRoleClaim, MasterUserToken> where T : Entity
    {
        public BaseRepository(UserMasterDBContext dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
