// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Auths;
    using Fsel.Identity.Domain.Models.CommandModels.Humans;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class HumanProfile : Profile
    {
        public HumanProfile()
        {
            CreateMap<Human, HumanModel>().IgnoreAllNonExisting();
            CreateMap<CreateHumanCommandModel, Human>().IgnoreAllNonExisting();
            CreateMap<UpdateHumanCommandModel, Human>().IgnoreAllNonExisting();

            CreateMap<SignUpCommandModel, Human>().IgnoreAllNonExisting();
        }
    }
}
