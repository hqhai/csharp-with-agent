// Copyright (c) Atlantic. All rights reserved.

using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Microsoft.AspNetCore.Identity;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }
    }
}
