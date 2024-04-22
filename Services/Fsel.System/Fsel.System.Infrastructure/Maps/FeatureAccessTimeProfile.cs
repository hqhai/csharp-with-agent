// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.FeatureAccessTimes;
    using Fsel.System.Domain.Models.EntityModels;

    public class FeatureAccessTimeProfile : Profile
    {
        public FeatureAccessTimeProfile()
        {
            CreateMap<FeatureAccessTime, FeatureAccessTimeModel>().IgnoreAllNonExisting();
            CreateMap<SaveFeatureAccessTimeCommandModel, FeatureAccessTime>()
                    .ForMember(dest => dest.CreatedUserId, opt => opt.MapFrom(src => src.UserId))
                    .IgnoreAllNonExisting();
        }
    }
}
