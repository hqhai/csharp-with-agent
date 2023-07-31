// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;

    public class UserSettingRepository : BaseRepository<UserSetting>, IUserSettingRepository
    {
        public UserSettingRepository(UserDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
