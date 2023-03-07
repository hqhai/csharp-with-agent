using Fsel.User.Domain.Entities;
using Fsel.User.Domain.IRepositories;
using Microsoft.AspNetCore.Identity;

namespace Fsel.User.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<Account> _userManager;

        public AccountRepository(UserManager<Account> userManager)
        {
            _userManager = userManager;
        }
    }
}