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
                .ForMember(d => d.AiCriteriaModel, o => o.Ignore())
                .IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigs, AiCriteriaModel>().IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigs, CreateAiCriteriaConfigCommandModel>().IgnoreAllNonExisting();
            CreateMap<UpdateSettingAiFeatureCommandModel, AICriteriaConfigs>().IgnoreAllNonExisting();
            CreateMap<CreateOrUpdateAiCriteriaCommand, AICriteriaConfigs>()
                .ForMember(d => d.AiPromptManagerId, opt => opt.MapFrom(s => s.AiPromptManagerId))
                .ForMember(d => d.SettingTemperature, opt => opt.MapFrom(s => s.SettingTemperature))
                .ForMember(d => d.SettingWordMaxLength, opt => opt.MapFrom(s => s.SettingWordMaxLength))
                .ForMember(d => d.SettingTopP, opt => opt.MapFrom(s => s.SettingTopP))
                .ForMember(d => d.SettingFrequency, opt => opt.MapFrom(s => s.SettingFrequency))
                .ForMember(d => d.SettingPresence, opt => opt.MapFrom(s => s.SettingPresence))
                .ForMember(d => d.MaximumNumber, opt => opt.MapFrom(s => s.MaximumNumber))
                .IgnoreAllNonExisting();

        }
    }
}
