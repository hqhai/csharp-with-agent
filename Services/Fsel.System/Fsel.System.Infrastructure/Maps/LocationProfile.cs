// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.EntityModels;

    public class LocationProfile : Profile
    {
        public LocationProfile()
        {
            CreateMap<Location, LocationModel>().IgnoreAllNonExisting();
            CreateMap<CrmLocation, LocationModel>()
                .ForMember(x => x.Id, x => x.MapFrom(n => n.GlobalId))
                .ForMember(x => x.ParentId, x => x.MapFrom(n => n.Parent != null ? n.Parent.GlobalId : default(Guid?)))
                .ForMember(x => x.Type, x => x.MapFrom(n => (Shared.Enums.EnumLocationType?)n.Level));
        }
    }
}
