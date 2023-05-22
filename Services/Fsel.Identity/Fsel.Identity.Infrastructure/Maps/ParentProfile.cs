// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.CommandModels.Users;
    using Fsel.Identity.Domain.Models.EntityModels;

    public class ParentProfile : Profile
    {
        public ParentProfile()
        {
            CreateMap<Parent, ParentModel>().IgnoreAllNonExisting();
            CreateMap<UpdateParentCommandModel, Parent>().IgnoreAllNonExisting();
            CreateMap<UpdateUserProfileCommandModel, Parent>().IgnoreAllNonExisting();
            CreateMap<Parent, UserProfileModel>().ForMember(m => m.Id, opt => opt.Ignore()).IgnoreAllNonExisting();
            CreateMap<Parent, ParentProfileModel>().IgnoreAllNonExisting();
        }
    }
}
