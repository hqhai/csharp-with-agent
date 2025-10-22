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
            CreateMap<CreateAiCriteriaConfigCommandModel, AICriteriaConfigs>().IgnoreAllNonExisting();
            CreateMap<UpdateAiCriteriaCommandModel, AICriteriaConfigs>().IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigsModel, AICriteriaConfigs>().IgnoreAllNonExisting();
            CreateMap<AICriteriaConfigs, AICriteriaConfigsModel>().IgnoreAllNonExisting();
        }
    }
}
