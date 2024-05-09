// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Identity.Domain.Entities;
    using Fsel.Shared.Enums;

    public interface IUserRepository
    {
        Task<User> GenerateUserDataAsync(User user, EnumRoleRegister role);
    }
}
