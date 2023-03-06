using Fsel.Common.ConfigSettings;
using Fsel.Core.Base;
using Fsel.User.Common.Helpers;
using Fsel.User.Common.Models.Entities;
using Fsel.User.Domain.Entities;
using Fsel.User.Domain.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
