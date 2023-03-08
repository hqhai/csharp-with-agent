using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Common.Models.Entities;
using Fsel.Identity.Domain.Entities;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserModel>().IgnoreAllNonExisting();
        }
    }
}