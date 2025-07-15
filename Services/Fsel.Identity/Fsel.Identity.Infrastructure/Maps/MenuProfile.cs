using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.Entities;
using Fsel.Shared.Models.ShareModels;

namespace Fsel.Identity.Infrastructure.Maps
{
    public class MenuProfile : Profile
    {
        public MenuProfile()
        {
            CreateMap<Menu, MenuModel>().IgnoreAllNonExisting();
        }
    }
}
