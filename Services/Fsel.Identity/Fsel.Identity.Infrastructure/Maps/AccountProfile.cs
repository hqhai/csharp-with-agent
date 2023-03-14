using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.Models.EntityModels;

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
