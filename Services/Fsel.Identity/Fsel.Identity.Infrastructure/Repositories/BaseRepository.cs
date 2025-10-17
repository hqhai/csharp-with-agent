// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Entities;

    public class BaseRepository<T> : BaseIdentityRepository<T, User, Role, Guid, UserClaimEntity, RoleClaimEntity, UserRole, UserLoginEntity, UserToken> where T : Entity
    {
        public BaseRepository(BaseIdentityDbContext<User, Role, Guid, UserClaimEntity, RoleClaimEntity, UserRole, UserLoginEntity, UserToken> dbContext, AuthContext authContext, IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
