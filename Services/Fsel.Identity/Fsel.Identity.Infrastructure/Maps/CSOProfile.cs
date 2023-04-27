// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class CSOProfile : Profile
    {
        public CSOProfile()
        {
            CreateMap<CSO, CSOModel>().IgnoreAllNonExisting();
            CreateMap<CreateUserCommandModel, CSO>().IgnoreAllNonExisting();
            CreateMap<UpdateUserCommandModel, CSO>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();

            CreateMap<UpdateUserProfileCommandModel, CSO>().IgnoreAllNonExisting();
        }
    }
}
