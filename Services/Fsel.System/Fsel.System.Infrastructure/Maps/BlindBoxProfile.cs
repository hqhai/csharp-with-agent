using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.System.Domain.Entities.BlindBoxs;
using Fsel.System.Domain.Models.EntityModels;

namespace Fsel.System.Infrastructure.Maps
{
    public class BlindBoxProfile : Profile
    {
        public BlindBoxProfile()
        {
            CreateMap<BlindBox, BlindBoxModel>().IgnoreAllNonExisting();
            CreateMap<BlindBoxChest, BlindBoxChestModel>().IgnoreAllNonExisting();
            CreateMap<BlindBoxChestConfig, BlindBoxChestConfigModel>().IgnoreAllNonExisting();
        }
    }
}
