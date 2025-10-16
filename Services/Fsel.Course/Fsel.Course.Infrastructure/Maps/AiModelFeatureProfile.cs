// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Domain.Models.EntityModels;

    public class AiModelFeatureProfile : Profile
    {
        public AiModelFeatureProfile()
        {
            CreateMap<AiModelFeature, AiFeatureModel>().IgnoreAllNonExisting();
        }
    }
}
