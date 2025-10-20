// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiModelFeature;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;

    public class AiModelFeatureProfile : Profile
    {
        public AiModelFeatureProfile()
        {
            CreateMap<AiModelFeature, AiFeatureModel>().IgnoreAllNonExisting()
                .ForMember(d => d.SubFeatures,  o => o.MapFrom(s => s.SubFeatures));
            CreateMap<CreateAiFeatureCommandModel, AiModelFeature>().IgnoreAllNonExisting();
            CreateMap<CreateAiFeatuerHasSubCommadModel, AiModelFeature>().IgnoreAllNonExisting();
            CreateMap<UpdateAiFeatureCommandModel, AiModelFeature>().IgnoreAllNonExisting();
            CreateMap<AiFeatureModel, AiModelFeature>().IgnoreAllNonExisting();

            CreateMap<AiModelFeature, AiSubFeatueModel>()
                .ForMember(d => d.TypeFeatureAi, o => o.MapFrom(s => s.TypeFeatureAi))
                .ForMember(d => d.UserRole, o => o.MapFrom(s => s.UserRole))
                .ForMember(d => d.Config, o => o.MapFrom(s => s.Config))
                .ForMember(d => d.Json, o => o.MapFrom(s => s.Json ?? s.JsonConfig));

        }
    }
}
