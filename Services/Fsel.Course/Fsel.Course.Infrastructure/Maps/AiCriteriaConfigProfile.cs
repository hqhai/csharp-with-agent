// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Maps
{
    using AutoMapper;
    using Core.Extensions;
    using Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.AiCriteriaConfig;
    using Fsel.Course.Domain.Models.EntityModels.AiPromptManagerModels;

    public class AiCriteriaConfigProfile : Profile
    {
        public AiCriteriaConfigProfile()
        {
            CreateMap<CreateAiCriteriaConfigCommandModel, AICriteriaConfigs>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.JsonConfig, o => o.MapFrom(s => s.JsonConfig))
                .ForMember(d => d.SettingAiJson, o => o.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<UpdateAiCriteriaCommand, AICriteriaConfigs>()
                .ForMember(d => d.JsonConfig, o => o.MapFrom(s => s.JsonConfig))
                .ForMember(d => d.SettingAiJson, o => o.Ignore())
                .ForMember(d => d.AiPromptManager, o => o.Ignore())
                .ForMember(d => d.AiPromptManager, o => o.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigsModel, AICriteriaConfigs>().IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigs, AICriteriaConfigsModel>()
                .ForMember(d => d.AiCriteriaModels, o => o.Ignore())
                .ForMember(d => d.AiModel, o => o.MapFrom(s => s.AiPromptManager != null ? s.AiPromptManager.AiModel : null))
                .IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigs, AiCriteriaModel>().IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigs, CreateAiCriteriaConfigCommandModel>().IgnoreAllNonExisting();
            CreateMap<UpdateSettingAiFeatureCommandModel, AICriteriaConfigs>().IgnoreAllNonExisting();
            CreateMap<CreateOrUpdateAiCriteriaCommandModel, AICriteriaConfigs>()
                .IgnoreAllNonExisting();
        }
    }
}
