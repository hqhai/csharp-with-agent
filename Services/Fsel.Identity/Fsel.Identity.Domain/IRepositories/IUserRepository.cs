// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public interface IUserRepository
    {
        DbContext DbContext { get; }

        IQueryable<User> Queryable { get; }

        Task<User> GenerateUserDataAsync(User user, EnumRoleRegister role);

        Task<User> GetUserByIdentity(string identity);
    }
}
