// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Entities;
    using Microsoft.AspNetCore.Identity;

    public class BaseRepository<T> : BaseIdentityRepository<T, User, Role, string, IdentityUserClaim<string>, IdentityRoleClaim<string>, UserToken> where T : Entity
    {
        public BaseRepository(BaseIdentityDbContext<User, Role, string, IdentityUserClaim<string>, IdentityRoleClaim<string>, UserToken> dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
