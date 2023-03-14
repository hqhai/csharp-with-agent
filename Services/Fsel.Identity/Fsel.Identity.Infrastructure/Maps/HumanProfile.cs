// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Humans;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class HumanProfile : Profile
    {
        public HumanProfile()
        {
            CreateMap<Human, HumanModel>().IgnoreAllNonExisting();
            CreateMap<CreateHumanCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<UpdateHumanCommandModel, Human>().IgnoreAllNonExisting();
        }
    }
}
