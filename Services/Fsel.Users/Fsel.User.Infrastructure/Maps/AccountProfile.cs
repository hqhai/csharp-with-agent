using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.User.Common.Models.Entities;
using Fsel.User.Domain.Entities;

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