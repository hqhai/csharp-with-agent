using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fsel.Core.Extensions;
using Fsel.System.Domain.Entities.BlindBoxs;
using Fsel.System.Domain.Models.CommandModels.ChatbotConfigs;
using Fsel.System.Domain.Models.EntityModels;

namespace Fsel.System.Infrastructure.Maps
{
    public class BlindBoxProfile : Profile
    {
        public BlindBoxProfile()
        {
            CreateMap<BlindBox, BlindBoxModel>().IgnoreAllNonExisting();
            CreateMap<BlindBoxChest, BlindBoxChestModel>().IgnoreAllNonExisting();
        }
    }
}
