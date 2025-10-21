// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiModelFeature;
    using Fsel.Course.Domain.Models.EntityModels.AiManagerModels;

    public class AiFeatureFeatureProfile : Profile
    {
        public AiFeatureFeatureProfile()
        {
            CreateMap<AIPromptConfigs, AiFeatureConfigModel>().IgnoreAllNonExisting()
                .ForMember(d => d.SubFeatures,  o => o.MapFrom(s => s.SubFeatures));
            CreateMap<CreateAiFeatureConfigCommandModel, AIPromptConfigs>().IgnoreAllNonExisting();
            CreateMap<CreateAiFeatuerHasSubCommadModel, AIPromptConfigs>().IgnoreAllNonExisting();
            CreateMap<UpdateAiFeatureCommandModel, AIPromptConfigs>().IgnoreAllNonExisting();
            CreateMap<AiFeatureConfigModel, AIPromptConfigs>().IgnoreAllNonExisting();

            CreateMap<AIPromptConfigs, AiSubFeatueModel>()
                .ForMember(d => d.TypeFeatureAi, o => o.MapFrom(s => s.TypeFeatureAi))
                .ForMember(d => d.UserRole, o => o.MapFrom(s => s.UserRole))
                .ForMember(d => d.Config, o => o.MapFrom(s => s.AiConfigSetting))
                .ForMember(d => d.Json, o => o.MapFrom(s => s.Json ?? s.JsonConfig));

        }
    }
}
