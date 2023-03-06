using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.User.Common.Models.Entities;
using Fsel.User.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.User.Infrastructure.Maps
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<Account, AccountModel>().IgnoreAllNonExisting();
        }
    }
}
